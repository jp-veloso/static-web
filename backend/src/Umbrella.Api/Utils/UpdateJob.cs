using System.Net;
using System.Net.Mail;
using Quartz;
using Umbrella.Api.Contexts;
using Umbrella.Api.Entities;

namespace Umbrella.Api.Utils;

public sealed class UpdateJob : IJob
{
    private readonly ExchangerSender _sender;

    public UpdateJob(ExchangerSender sender)
    {
        _sender = sender;
    }

    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("SENDING TO UPDATE QUEUE");

        DebugEmail();

        RepositoryContext db = new();

        List<Insurer> insurers = db.Insurers.Where(x => x.Active && x.HasIntegration)
                                   .ToList();

        InsPayload payload = new();

        foreach (Insurer? insurer in OnlyValid(insurers))
        {
            payload.AddValue(insurer.Id, insurer.Name);
        }

        _sender.SendToRabbit(payload);

        return Task.FromResult(true);
    }

    private static IEnumerable<Insurer> OnlyValid(IEnumerable<Insurer> insurers)
    {
        string[] hasUpdate = {"Junto Seguros", "Sombrero", "BMG", "Excelsior", "Essor", "Tokio", "Berkley", "JNS"};
        return insurers.Where(x => hasUpdate.Contains(x.Name));
    }

    private static void DebugEmail()
    {
        SmtpClient client = new("smtp.gmail.com")
                            {
                                EnableSsl = true,
                                Credentials =
                                    new NetworkCredential("atendimento@grantoseguros.com",
                                                          "EhfI*6~93sEk"),
                                Port = 587
                            };

        MailMessage message = new();
        message.From = new MailAddress("atendimento@grantoseguros.com");
        message.To.Add(new MailAddress("pereira.pedro@grantoseguros.com"));

        message.Subject = "[DEBUG] Atualização de cadastros vencidos";
        message.Body = $"Hoje {DateTime.Now:G} foi enviada uma mensagem a fila para atualizar os registros vencidos";

        client.Send(message);
    }
}