using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using iText.StyledXmlParser.Css.Parse;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Umbrella.Api.Contexts;
using Umbrella.Api.Entities;
using Umbrella.Cosmos.Repository.Repositories;
using ResponseHeaders = Azure.Core.ResponseHeaders;

namespace Umbrella.Api.Services;

public class UserService
{
    private const string INTERNALS_URL = "https://gcb-api.thankfulhill-40cbc43a.brazilsouth.azurecontainerapps.io";

    private readonly IRepository<MetricRecord> _cacheRepository;

    public UserService(IRepositoryFactory repositoryFactory)
    {
        _cacheRepository = repositoryFactory.RepositoryOf<MetricRecord>();
    }

    public object Me(int id)
    {
        using RepositoryContext db = new();

        DateTime today = DateTime.Now;

        return new
               {
                   period = $"{today:G}",
                   kpis = new {clients = BaseClients(db, today)}
               };
    }

    public object MePublic()
    {
        using RepositoryContext db = new();

        DateTime today = DateTime.Now;

        return new
               {
                   period = $"{today:G}",
                   kpis = EvaluateKpis(db, today)
               };
    }

    public async Task<object> GetKpisRecordsAsync(DateTime date)
    {
        await using RepositoryContext context = new();

        var t = GetOsaykValues(date);

        string period = date.Year + ":" + date.Month;

        float ltv = GetLtv(context, 365, date);
        float monthCommission = GetRecipe(context, date);
        int clients = BaseClients(context, date);

        JsonElement element = await t;
        
        float cac = GetCac(element.GetProperty("outgoing").GetSingle(), date);
        float recipe = element.GetProperty("recipe").GetSingle();
        
        int trim = (date.Month - 1) / 3 + 1;
        
        DateTime begin = new DateTime(date.Year, (trim - 1) * 3 + 1, 1);
        DateTime endNps = begin.AddDays(90);
        
        List<ScoreRequest> requests = context.ScoreRequests.Include(x => x.Score).Where(x => x.CreatedAt >= begin && x.CreatedAt <= endNps).ToList();
        int[] scores = requests.Where(x => x.Score != null).Select(x => x.Score!.Value).ToArray();
        
        MetricNps? nps = MetricNps.GenerateNps(scores, requests.Count);
        
        MetricRecord record = new MetricRecord()
        {
            Period = period,
            Cac = cac,
            Ltv = ltv,
            Clients = clients,
            MonthCommission = monthCommission,
            Nps = nps,
            Recipe = recipe,
            CreatedAt = date
        };
        
        var result = await _cacheRepository.GetAsync(x => x.Period != period);

        await SaveRecordAsync(record);
        
        return result.Append(record);
    }

    private async Task SaveRecordAsync(MetricRecord record)
    {
        var result = (await _cacheRepository.GetAsync(x => x.Period == record.Period)).ToList();

        if (!result.Any())
        {
            await _cacheRepository.CreateAsync(record);
        }
        else
        {
            MetricRecord persisted = result.First();
            record.Id = persisted.Id;

            await _cacheRepository.UpdateAsync(record);
        }
    }

    private static async Task<JsonElement> GetOsaykValues(DateTime initial)
    {
        using HttpClient client = new();

        HttpRequestMessage message = new(HttpMethod.Get, INTERNALS_URL + "/info");
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Headers.Add("x-key", "12e8ne#$k21");
        message.Version = HttpVersion.Version11;
        string json = $"{{\"initial\":\"{initial:yyyy-MM-dd}\"}}";
        message.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.SendAsync(message);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return document.RootElement;
    }

    private static int GetClientsWithPeriod(DateTime end, int interval)
    {
        DateTime begin = end.AddDays(-interval);

        using RepositoryContext db = new();

        int amount = db.Database.SqlQueryRaw<int>(@$"SELECT COUNT(DISTINCT c.id) AS Value 
                                                   FROM portal.Client c INNER JOIN portal.Issue e ON c.id = e.clientFK 
                                                    WHERE e.issuedAt >= '{begin}' AND e.issuedAt < '{end}' 
                                                    AND NOT EXISTS (SELECT * FROM portal.Issue WHERE clientFK = c.id AND issuedAt < '{begin}')")
                       .First();

        return amount;
    }

    private static float GetCac(double outgoing, DateTime end)
    {
        return (float) outgoing / GetClientsWithPeriod(end, 365);
    }

    private static object EvaluateKpis(RepositoryContext context, DateTime today)
    {
        JsonElement element = GetOsaykValues(today).Result;

        double ltv = GetLtv(context, 365, today);
        double monthCommission = GetRecipe(context, today);
        object nps = GetNps(context, today);
        double cac = GetCac(element.GetProperty("outgoing").GetDouble(), today);
        double recipe = element.GetProperty("recipe").GetDouble();
        int clients = BaseClients(context, today);

        return new
               {
                   ltv, monthCommission, recipe, nps, cac, clients
               };
    }

    private static float GetRecipe(RepositoryContext context, DateTime end)
    {
        DateTime begin = new(end.Year, end.Month, 1);

        List<Issue> issues = context.InsuranceIssued.Where(x => x.IssuedAt >= begin && x.IssuedAt <= end).ToList();

        return issues.Aggregate(0F, (d, issue) => (float) (d + issue.Commission * issue.Bounty));
    }

    private static object GetNps(RepositoryContext context, DateTime end)
    {
        int trim = (end.Month - 1) / 3 + 1;
        DateTime begin = new DateTime(end.Year, (trim - 1) * 3 + 1, 1);

        List<ScoreRequest> requests = context.ScoreRequests.Include(x => x.Score)
                                             .Where(x => x.CreatedAt >= begin)
                                             .ToList();

        int[] scores = requests.Where(x => x.Score != null)
                               .Select(x => x.Score!.Value)
                               .ToArray();

        if (scores.Length == 0)
        {
            return 0.0;
        }

        int promoters = scores.Count(x => x  >= 9);
        int detractors = scores.Count(x => x <= 6);

        return new
               {
                   value = (double) (promoters - detractors) / scores.Length * 100,
                   promoters,
                   detractors,
                   neutral = scores.Length    - promoters - detractors,
                   notAnswer = requests.Count - scores.Length
               };
    }

    private static float GetLtv(RepositoryContext context, int interval, DateTime end)
    {
        DateTime begin = end.AddDays(-interval);

        List<Client> clients = context.Clients
                              .FromSql($"SELECT c.id, c.cnpj, c.createdAt, c.name, c.segment, c.pipe FROM portal.Client c INNER JOIN portal.Issue i ON c.Id = i.clientFK WHERE (i.IssuedAt >= {begin} AND i.IssuedAt <= {end}) GROUP BY c.id, c.cnpj, c.name, c.segment, c.createdAt, c.pipe")
                              .Include(x => x.Issued)
                              .ToList();

        double totalCommission = 0F;
        double totalWons = 0F;
        double totalTime = 0F;

        foreach (Client? client in clients)
        {
            ICollection<Issue> wons = client.Issued;

            totalWons += wons.Count;
            totalCommission += wons.Aggregate(0.0, (f, issue) => f + issue.Commission * issue.Bounty);

            Issue first = wons.OrderBy(x => x.IssuedAt).First();

            float currentTime = (float) end.Subtract(first.IssuedAt).Days / 365;

            if (currentTime < 1)
            {
                currentTime = 1;
            }

            totalTime += currentTime;
        }

        return (float) (totalCommission / totalWons * (totalWons / clients.Count) * (totalTime / clients.Count));
    }

    private static int BaseClients(DbContext db, DateTime dateTime)
    {
        return db.Database.SqlQueryRaw<Int32>($"SELECT COUNT(DISTINCT c.id) AS Value FROM portal.Client c INNER JOIN portal.Issue e ON c.id = e.clientFK WHERE e.issuedAt <= '{dateTime}'").Single();
    }
}