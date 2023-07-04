using System.Media;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using RiskAnalysis;
using Umbrella.Api.Contexts;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.ProposalResources;
using Umbrella.Api.ProposalResources.Enums;
using Umbrella.Api.Resources.Exceptions;
using Umbrella.Api.Services.Exceptions;

namespace Umbrella.Api.Services;

public class EnrollmentService
{
    private readonly RiskAnalysisDataService _analysis;

    public EnrollmentService(RiskAnalysisDataService analysis)
    {
        _analysis = analysis;
    }

    private static StandardError NotFound(int clientId, int insurerId)
    {
        return new StandardError
               {
                   Error = "Enrollment not found",
                   Message = $"Entity with client_id = {clientId} and insurer_id = {insurerId} not found",
                   Status = 404,
                   Timestamp = DateTime.Now
               };
    }

    public List<Proposal> GenerateProposals(int clientId, ProposalInfo info)
    {
        using RepositoryContext db = new();

        ICollection<Enrollment> enrollments = db.Enrollments
                                                .Include(x => x.Client)
                                                .Include(x => x.Policyholders)
                                                .Include(x => x.Insurer)
                                                    .ThenInclude(x => x.ProposalParameters)
                                                .Where(x => x.Client.Id == clientId)
                                                .ToList();

        ProposalService service = new(info, _analysis);

        foreach (Enrollment? enrollment in enrollments)
        {
            ProposalParameters? parameters = enrollment.Insurer.ProposalParameters.SingleOrDefault(x => x.ProposalType == info.Contract);

            string rule = FindRule(enrollment.Insurer.Id);

            if (string.IsNullOrEmpty(rule) || parameters == null)
            {
                continue;
            }

            service.UseParameters(parameters).ChangeRule(x => x.Rule == rule).DoProposal(enrollment);
        }

        service.DoDefault();

        return service.GetProposals();
    }

    private string FindRule(int insurerId)
    {
        return insurerId switch
        {
            (int)ProposalInsurer.JUNTO or (int)ProposalInsurer.AVLA => "TABLE",
            (int)ProposalInsurer.PORTO or (int)ProposalInsurer.JNS or (int)ProposalInsurer.EZZE => string.Empty,
            _ => "COMMON"
        };
    }

    public EnrollmentDTO Update(int clientId, int insurerId, EnrollmentDTO payload)
    {
        using RepositoryContext db = new();

        Enrollment? enrollment = db.Enrollments.Include(x => x.Insurer)
                                   .Include(x => x.Policyholders)
                                   .SingleOrDefault(x => x.Client.Id == clientId && x.Insurer.Id == insurerId);

        if (enrollment == null)
        {
            throw new ServiceException("Enrollment not found", NotFound(clientId, insurerId));
        }

        enrollment.ExpireAt = payload.ExpireAt;
        enrollment.Status = payload.Status;
        enrollment.Warn = payload.Warn;
        enrollment.Rating = payload.Rating;
        enrollment.IsActive = payload.IsActive;

        if (payload.Takers != null)
        {
            foreach (TakerDTO? value in payload.Takers)
            {
                Taker taker = enrollment.Policyholders.FirstOrDefault(x => x.Category == value.Category) ??
                              new Taker(value.Category);

                taker.Balance = value.Balance;
                taker.Limit = value.Limit;
                taker.Rate = value.Rate;

                enrollment.Policyholders.Remove(taker);
                enrollment.Policyholders.Add(taker);
            }

            enrollment.Policyholders = enrollment.Policyholders
                                                 .Where(x => payload.Takers.Any(y => y.Category == x.Category))
                                                 .ToList();
        }

        db.Update(enrollment);
        db.SaveChanges();

        return new EnrollmentDTO(enrollment, enrollment.Insurer, null, enrollment.Policyholders) {ClientId = clientId};
    }

    public EnrollmentDTO Insert(int clientId, EnrollmentDTO payload)
    {
        using RepositoryContext db = new();

        Insurer? insurer = db.Insurers.Find(payload.InsurerId);
        Client? client = db.Clients.Find(clientId);

        if (insurer == null || client == null)
        {
            throw new ServiceException("Enrollment not found", NotFound(clientId, payload.InsurerId));
        }

        Enrollment enrollment = new(client, insurer)
                                {
                                    Rating = payload.Rating,
                                    ExpireAt = payload.ExpireAt,
                                    Status = Status.CREATED,
                                    IsActive = true,
                                    Policyholders = new List<Taker>()
                                };

        if (payload.Takers != null)
        {
            foreach (TakerDTO? taker in payload.Takers)
            {
                enrollment.Policyholders.Add(new Taker(taker.Limit, taker.Balance, taker.Category, taker.Rate));
            }
        }

        db.Enrollments.Add(enrollment);
        db.SaveChanges();

        return new EnrollmentDTO(enrollment, insurer, client, enrollment.Policyholders);
    }

    public void Delete(int clientId, int insurerId)
    {
        using RepositoryContext db = new();

        Enrollment? enrollment = db.Enrollments.Find(clientId, insurerId);

        if (enrollment == null)
        {
            throw new ServiceException("Enrollment not found", NotFound(clientId, insurerId));
        }

        db.Enrollments.Remove(enrollment);
        db.SaveChanges();
    }

    private string UseAssertivaScore(string cnpj)
    {
        const string username = "A2gIbeQ2W+YkSFssHmkGxulomR2XA7WT6xU9y2jIIZB+5yLN5O1tVYl4ex811b/Mr7B/FDtqqtZvq/Alz5G1aw==";
        const string password = "nK1nEBPt28JHw2mmapJ/PdSteDbaliHtbJIWJj91lMpZQ/1sPQHkgIX2x4WAUWeE0Q5VsrxCY02EJ2uBo7UbfQ==";

        using var client = new HttpClient(){ BaseAddress = new Uri("https://api.assertivasolucoes.com.br") };
        
        HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "/oauth2/v3/token");
            
        message.Headers.Authorization = new AuthenticationHeaderValue("Basic", $"{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}");

        message.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type" , "client_credentials")
        });
        
        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            
        HttpResponseMessage response = client.Send(message);

        string body = response.Content.ReadAsStringAsync().Result;

        JsonNode node = JsonSerializer.Deserialize<JsonNode>(body)!;

        string authToken = node["access_token"]!.ToString();

        client.DefaultRequestHeaders.Add("Authorization", authToken);
            
        HttpResponseMessage response2 = client.GetAsync($"/score/v3/pj/recupere/{cnpj}?idFinalidade=5").Result;

        body = response2.Content.ReadAsStringAsync().Result;
        node = JsonSerializer.Deserialize<JsonNode>(body)!;

        return node["resposta"]!["score"]!["pontos"]!.ToString();
    }

    public IEnumerable<EnrollmentDTO> FindAll(int clientId)
    {
        using RepositoryContext db = new();

        List<Enrollment> enrollments = db.Enrollments.Include(x => x.Insurer)
                                         .Include(x => x.Policyholders)
                                         .Where(x => x.Client.Id == clientId)
                                         .ToList();

        var ratings = enrollments
            .Where(x => x.Status == Status.CREATED && !string.IsNullOrEmpty(x.Rating))
            .Select(x => x.Rating!.Replace(" ", "").Replace("SERASA:", ""))
            .Distinct()
            .ToList();
        
        //bool usedAssertiva = !ratings.Any();
        bool usedAssertiva = false;
        
        if (usedAssertiva)
        {
            string cnpj = db.Clients.Find(clientId)?.Cnpj ?? throw new ServiceException("Client not found", NotFound(clientId, 0));
            
            ratings.Add(UseAssertivaScore(cnpj));
        }

        return enrollments.Select(x =>
        {
            if (x.Status != Status.ERROR)
            {
                return new EnrollmentDTO(x, x.Insurer, null, x.Policyholders);
            }
            else
            {
                List<VirtualRateDTO> virtualRates = new List<VirtualRateDTO>();

                foreach (var rating in ratings)
                {
                    var result = _analysis.Predict(x.Insurer.Id, rating, true);

                    VirtualRateDTO dto = new VirtualRateDTO()
                    {
                        RatingSource = usedAssertiva ? "score" : "cia",
                        VirtualRate = result,
                        Rating = rating
                    };

                    virtualRates.Add(dto);
                }

                return new EnrollmentDTO(x, x.Insurer, x.Policyholders, virtualRates);
            }
            
        }).OrderBy(x => x.Insurer!.Name).ToList();
    }
}