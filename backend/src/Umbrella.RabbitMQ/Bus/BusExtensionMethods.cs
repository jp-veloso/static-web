using System;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Umbrella.RabbitMQ.Buffers;

namespace Umbrella.RabbitMQ.Bus;

public static class BusExtensionMethods
{
    public static IServiceCollection RegisterBus(this IServiceCollection services)
    {
        services.AddSingleton(sp => new RingBuffer<IModel>(2, sp.GetRequiredService<IModel>, model => model.IsOpen,
                                                           _ => { }, TimeSpan.FromSeconds(10)));
        services.AddSingleton<Bus>();
        services.AddTransient<IEventBus>(sp => sp.GetRequiredService<Bus>());
        services.AddTransient<ICommandBus>(sp => sp.GetRequiredService<Bus>());

        return services;
    }
}