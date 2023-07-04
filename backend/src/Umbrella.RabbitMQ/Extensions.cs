using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Umbrella.RabbitMQ.Configuration;
using Umbrella.RabbitMQ.Rpc;
using Umbrella.RabbitMQ.Serialization;

namespace Umbrella.RabbitMQ;

public static partial class Extensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services,
                                                 Action<RabbitMQConfigurationBuilder> action)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        RabbitMQConfigurationBuilder builder = new(services);

        action(builder);

        builder.Build();

        return services;
    }

    public static IServiceCollection AddAmqpRpcClient(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddTransient(sp => sp.GetRequiredService<IConnection>()
                                      .CreateModel());

        services.AddScoped(sp => new SimpleAmqpRpc(sp.GetRequiredService<IModel>(),
                                                   sp.GetRequiredService<IAmqpSerializer>(),
                                                   sp.GetRequiredService<ActivitySource>(),
                                                   TimeSpan.FromMinutes(5) //default, but can be override
                                                  ));

        return services;
    }
}