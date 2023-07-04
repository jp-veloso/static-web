using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Umbrella.Api.Contexts;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.Resources.Exceptions;
using Umbrella.Api.Services.Exceptions;
using Umbrella.Api.Utils;
using Umbrella.Api.Utils.Pagination;

namespace Umbrella.Api.Services;

public class ClientService
{
    private readonly ExchangerSender _sender;

    public ClientService(ExchangerSender sender)
    {
        _sender = sender;
    }

    public ClientDTO CreateByCnpj(string cnpj, string name, int companyId)
    {
        if (!TextUtil.IsCnpj(cnpj))
        {
            throw new ServiceException("Invalid CNPJ", new StandardError
                                                       {
                                                           Error = "Invalid CNPJ format",
                                                           Timestamp = DateTime.Now,
                                                           Status = 400,
                                                           Message = $"{cnpj} is not a valid cnpj"
                                                       });
        }

        cnpj = TextUtil.UnformatCNPJ(cnpj);

        using RepositoryContext ctx = new();

        Client? client = ctx.Clients.SingleOrDefault(x => x.Cnpj.Equals(cnpj));

        if (client == null)
        {
            ClientDTO dto = Insert(new ClientDTO { Cnpj = cnpj, Name = name, PipeId = $"{companyId}"}, true);
            return dto;
        }

        ReloadEnrollments(client.Id, Array.Empty<int>());
        return FindById(client.Id);
    }

    public static StandardError NotFound(int clientId)
    {
        return new StandardError
               {
                   Error = "Client not found",
                   Message = $"Entity with client_id = {clientId} not found",
                   Status = 404,
                   Timestamp = DateTime.Now
               };
    }

    private static Expression<Func<Client, bool>> BuildLikeExpression(string filter)
    {
        filter = filter.Replace(".", "")
                       .Replace("-", "")
                       .Replace("/", "");
        return client => EF.Functions.Like(client.Cnpj, $"%{filter}%") || EF.Functions.Like(client.Name, $"%{filter}%");
    }

    public Page<ClientDTO> FindAllPaged(Pageable pageable, string? filter)
    {
        using RepositoryContext db = new();

        Page<Client> clients = !string.IsNullOrEmpty(filter)
                                   ? db.Clients.Where(BuildLikeExpression(filter))
                                       .ToPaged(pageable)
                                   : db.Clients.ToPaged(pageable);

        return clients.Convert(x => new ClientDTO(x));
    }

    public ClientDTO FindById(int id)
    {
        using RepositoryContext db = new();

        Client? client = db.Clients.SingleOrDefault(x => x.Id == id);

        if (client == null)
        {
            throw new ServiceException("Client not found", NotFound(id));
        }

        ClientDTO found = new(client);
        IsClient(db, found, id);

        return found;
    }

    private static void IsClient(RepositoryContext context, ClientDTO dto, long id)
    {
        DateTime period = DateTime.Now.AddDays(-365);

        Issue? onPeriod = context.InsuranceIssued
            .FromSqlRaw($"SELECT * FROM [portal].[Issue] i WHERE i.clientFK = {id} ORDER BY i.issuedAt DESC OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY")
                                 .Include(x => x.Users)
                                 .Include(x => x.Insurer)
                                 .FirstOrDefault();

        if (onPeriod == null)
        {
            return;
        }

        dto.LastIssue = new IssueDTO(onPeriod);
        dto.IsClient = true;
        dto.Active = onPeriod.IssuedAt >= period;
        dto.LastIssue.Client = null;
    }

    public void DeleteById(int id)
    {
        using RepositoryContext db = new();

        Client? client = db.Clients.Find(id);

        if (client == null)
        {
            throw new ServiceException("Client not found", NotFound(id));
        }

        db.Clients.Remove(client);
        db.SaveChanges();
    }

    public ClientDTO Insert(ClientDTO payload, bool withEnrollments)
    {
        using RepositoryContext db = new();

        Client client = new()
                        {
                            Cnpj = TextUtil.UnformatCNPJ(payload.Cnpj),
                            Segment = payload.Segment ?? Segment.LOW_TOUCH,
                            CreatedAt = DateTime.Now,
                            Pipe = payload.PipeId
                        };

        client.Name = string.IsNullOrEmpty(payload.Name)
                          ? CallBrazilAPI(client.Cnpj)
                           .GetProperty("razao_social")
                           .ToString()
                          : payload.Name;

        db.Clients.Add(client);
        db.SaveChanges();

        if (!withEnrollments)
        {
            return FindById(client.Id);
        }

        client.Enrollments = new List<Enrollment>();

        List<Insurer> insurers = db.Insurers.Where(entity => entity.HasIntegration && entity.Active)
                                   .ToList();

        insurers.ForEach(insurer =>
                         {
                             Enrollment enrollment = new(client, insurer);
                             client.Enrollments.Add(enrollment);
                         });

        db.SaveChanges();

        SendInformation(insurers, client);

        return FindById(client.Id);
    }

    public void ReloadEnrollments(int clientId, int[] toReload)
    {
        using RepositoryContext db = new();

        Client? client = db.Clients.Include(x => x.Enrollments)
                           .ThenInclude(x => x.Insurer)
                           .SingleOrDefault(x => x.Id == clientId);

        if (client == null)
        {
            throw new ServiceException("Client not found", NotFound(clientId));
        }

        List<Insurer> insurers = db.Insurers.Where(entity => entity.HasIntegration && entity.Active)
                                   .ToList();

        if (toReload.Length > 0)
        {
            insurers = insurers.Where(entity => toReload.Contains(entity.Id))
                               .ToList();
        }

        insurers.ForEach(insurer =>
                         {
                             Enrollment? enrollment =
                                 client.Enrollments.FirstOrDefault(x => x.Insurer.Id == insurer.Id);

                             if (enrollment != null)
                             {
                                 enrollment.Status = Status.PREPARING;
                             }
                             else
                             {
                                 db.Enrollments.Add(new Enrollment(client, insurer));
                             }
                         });

        db.SaveChanges();

        insurers.ForEach(x => Console.WriteLine(x.Name));

        SendInformation(insurers, client);
    }

    private void SendInformation(List<Insurer> insurers, Client client)
    {
        insurers.ForEach(x => { Console.WriteLine($"Sending to [{x.Name}] with enrollment"); });

        EnrollPayload toSend = new();
        insurers.ForEach(x => toSend.AddValue(client.Id, x.Id, client.Cnpj));

        _sender.SendToRabbit(toSend);
    }

    private static JsonElement CallBrazilAPI(string cnpj)
    {
        using HttpClient client = new() {BaseAddress = new Uri("https://brasilapi.com.br")};
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/cnpj/v1/" + cnpj);
        HttpResponseMessage response = client.Send(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException("Brazil API Failed", new StandardError
                                                            {
                                                                Error =
                                                                    "Error trying to get cnpj information",
                                                                Message =
                                                                    $"Brazil API returned error with cnpj {cnpj}, {response.RequestMessage?.RequestUri}",
                                                                Timestamp = DateTime.Now,
                                                                Status = 400
                                                            });
        }

        JsonDocument document = JsonDocument.Parse(response.Content.ReadAsStringAsync()
                                                           .Result);
        return document.RootElement;
    }

    public byte[] GenerateAppointment(int id, int[] ints, string path)
    {
        using RepositoryContext db = new();

        Client? client = db.Clients.Find(id);

        if (client == null)
        {
            throw new ServiceException("Not found", NotFound(id));
        }

        JsonElement brazilResponse = CallBrazilAPI(client.Cnpj);

        List<Insurer> insurers = ints.Length == 0
                                     ? db.Insurers.Where(x => x.Active)
                                         .ToList()
                                     : db.Insurers.Where(x => ints.Contains(x.Id) && x.Active)
                                         .ToList();

        string resume = File.ReadAllText(path + "/Assets/nomeacao.txt");

        resume = resume.Replace("{{empresa}}", brazilResponse.GetProperty("razao_social")
                                                             .ToString())
                       .Replace("{{cnpj}}", TextUtil.FormatCNPJ(brazilResponse.GetProperty("cnpj")
                                                                              .ToString()))
                       .Replace("{{cidade}}", brazilResponse.GetProperty("municipio")
                                                            .ToString())
                       .Replace("{{uf}}", brazilResponse.GetProperty("uf")
                                                        .ToString())
                       .Replace("{{data}}", DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("pt-BR")));

        return GeneratePDF(resume, path, insurers);
    }

    private static byte[] GeneratePDF(string resume, string path, IEnumerable<Insurer> items)
    {
        PdfFont MULISH_BOLD = PdfFontFactory.CreateFont(FontProgramFactory.CreateFont(path + "/Assets/Mulish-Bold.ttf"),
                                                        PdfEncodings.WINANSI);
        PdfFont MULISH_REGULAR =
            PdfFontFactory.CreateFont(FontProgramFactory.CreateFont(path + "/Assets/Mulish-Regular.ttf"),
                                      PdfEncodings.WINANSI);
        ImageData BACKGROUND = ImageDataFactory.Create(path + "/Assets/background.png");

        using MemoryStream stream = new();

        using (PdfDocument pdf = new(new PdfWriter(stream)))
        {
            PdfCanvas canvas = new(pdf.AddNewPage());
            canvas.AddImageFittedIntoRectangle(BACKGROUND, PageSize.A4, false);

            Document document = new(pdf);
            document.SetFont(MULISH_REGULAR);
            document.SetMargins(130, 60, 0, 60);

            List<TabStop> tabStops = new() {new TabStop((PageSize.A4.GetWidth() - 120) / 2, TabAlignment.CENTER)};

            Paragraph paragraph = new Paragraph().SetFontSize(24)
                                                 .SetFont(MULISH_BOLD)
                                                 .AddTabStops(tabStops)
                                                 .Add(new Tab())
                                                 .Add("CARTA DE NOMEAÇÃO");

            Paragraph text = new Paragraph(resume).SetTextAlignment(TextAlignment.JUSTIFIED)
                                                  .SetFontSize(13);

            document.Add(paragraph);
            document.Add(text);
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            PdfCanvas c2 = new(pdf.GetPage(2));
            c2.AddImageFittedIntoRectangle(BACKGROUND, PageSize.A4, false);

            List<Insurer> enumerable = items.ToList();

            Table table = new Table(2).UseAllAvailableWidth();

            for (int i = 0; i < enumerable.Count(); i += 2)
            {
                List<Insurer> subList = enumerable.GetRange(i, Math.Min(2, enumerable.Count - i));

                subList.ForEach(x =>
                                {
                                    Cell c = new();

                                    c.Add(new Paragraph(x.RealName).SetFont(MULISH_BOLD)
                                                                   .SetFontSize(10));
                                    c.Add(new Paragraph(TextUtil.FormatCNPJ(x.Cnpj!)));
                                    c.SetTextAlignment(TextAlignment.LEFT);
                                    c.SetFontSize(9);
                                    c.SetBorder(Border.NO_BORDER);

                                    table.AddCell(c);
                                });
            }

            document.Add(table);
        }

        return stream.ToArray();
    }
}