using System;
using System.Diagnostics;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Umbrella.RabbitMQ.Serialization;

namespace Umbrella.RabbitMQ.Configuration;

public class RabbitMQConfigurationBuilder
{
    private readonly IServiceCollection services;
    private string configurationPrefix = "RABBITMQ";
    private int connectMaxAttempts = 8;

    private Func<int, TimeSpan> produceWaitConnectWait =
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));

    public RabbitMQConfigurationBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public RabbitMQConfigurationBuilder WithSerializer<T>() where T : class, IAmqpSerializer
    {
        services.AddSingleton<IAmqpSerializer, T>();
        return this;
    }

    public RabbitMQConfigurationBuilder WithConfigurationPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        configurationPrefix = prefix;
        return this;
    }

    public RabbitMQConfigurationBuilder WithConnectMaxAttempts(int maxAttempts,
                                                               Func<int, TimeSpan> waitConnectWait = null)
    {
        if (maxAttempts < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts),
                                                  "ConnectMaxAttempts must bem greater or equal zero.");
        }

        if (waitConnectWait == null)
        {
            throw new ArgumentNullException(nameof(waitConnectWait));
        }

        connectMaxAttempts = maxAttempts;
        produceWaitConnectWait = waitConnectWait;

        return this;
    }

    public void Build()
    {
        services.AddTransient(sp => sp.GetRequiredService<IConnection>()
                                      .CreateModel());

        services.AddSingleton(sp =>
                              {
                                  ConnectionFactory factory = new();
                                  sp.GetRequiredService<IConfiguration>()
                                    .Bind(configurationPrefix, factory);
                                  return factory;
                              });

        services.AddSingleton(sp => Policy.Handle<BrokerUnreachableException>()
                                          .WaitAndRetry(connectMaxAttempts, retryAttempt =>
                                                                            {
                                                                                TimeSpan wait =
                                                                                    produceWaitConnectWait(retryAttempt);
                                                                                Console
                                                                                   .WriteLine($"Can't create a connection with RabbitMQ. We wil try again in {wait.Humanize()}.");
                                                                                return wait;
                                                                            })
                                          .Execute(() =>
                                                   {
                                                       Debug.WriteLine("Trying to create a connection with RabbitMQ");

                                                       IConnection connection = sp
                                                          .GetRequiredService<ConnectionFactory>()
                                                          .CreateConnection();

                                                       Console
                                                          .WriteLine(@$"Connected on RabbitMQ '{connection}' with name '{connection.ClientProvidedName}'. 
....Local Port: {connection.LocalPort}
....Remote Port: {connection.RemotePort}
....cluster_name: {connection.ServerProperties.AsString("cluster_name")}
....copyright: {connection.ServerProperties.AsString("copyright")}
....information: {connection.ServerProperties.AsString("information")}
....platform: {connection.ServerProperties.AsString("platform")}
....product: {connection.ServerProperties.AsString("product")}
....version: {connection.ServerProperties.AsString("version")}");

                                                       return connection;
                                                   }));
    }
}