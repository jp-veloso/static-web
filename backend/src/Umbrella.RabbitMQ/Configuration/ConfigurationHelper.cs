using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Umbrella.RabbitMQ.Configuration;

public static class ConfigurationHelper
{
    public static T CreateConfiguredObject<T>(this IConfiguration configuration, string key) where T : new()
    {
        T returnValue = new();

        configuration.Bind(key, returnValue);

        return returnValue;
    }

    public static void ConfigureRabbitMQ(this IHost host)
    {
        using IModel model = host.Services.GetRequiredService<IModel>();

        CreateQueue("gcb_insurers-integration", "eventos", "topic", "insurers-integration", model);
        CreateQueue("gcb_insurers-update", "eventos", "topic", "insurers-update", model);
        CreateShovelEnv(model);
    }

    private static void CreateQueue(string queueName, string exchangeName, string exchangeType, string routingKey,
                                    IModel model)
    {
        CreateExchange(exchangeName, exchangeType, model);

        string nomeFilaProcessamento = $"{queueName}";
        string nomeFilafalha = $"{queueName}_failure";

        model.QueueDeclare(nomeFilafalha, true, false, false, null);

        model.QueueDeclare(nomeFilaProcessamento, true, false, false,
                           new Dictionary<string, object>
                           {{"x-dead-letter-exchange", ""}, {"x-dead-letter-routing-key", nomeFilafalha}});

        model.QueueBind(nomeFilaProcessamento, exchangeName, routingKey);
    }

    private static void CreateShovelEnv(IModel model)
    {
        const string routerExchangeName = "Duplicate_Router";
        const int replicas = 2;

        model.QueueDeclare("Duplicate_Entrypoint", true, false, false, null);
        model.ExchangeDeclare(routerExchangeName, "fanout", true, false, null);

        for (int i = 1; i <= replicas; i++)
        {
            model.QueueDeclare($"Duplicate_Set{i}", true, false, false, null);
            model.QueueBind($"Duplicate_Set{i}", routerExchangeName, "");
        }
    }

    private static void CreateExchange(string exchangeName, string type, IModel model)
    {
        string unroutedName = $"{exchangeName}_unrouted";

        model.ExchangeDeclare(unroutedName, "fanout", true, false, null);

        model.QueueDeclare(unroutedName, true, false, false, null);

        model.QueueBind(unroutedName, unroutedName, "");

        model.ExchangeDeclare(exchangeName, type, true, false,
                              new Dictionary<string, object> {{"alternate-exchange", unroutedName}});
    }
}