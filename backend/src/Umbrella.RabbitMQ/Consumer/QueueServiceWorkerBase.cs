using System;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Umbrella.RabbitMQ.Consumer;

public abstract class QueueServiceWorkerBase : BackgroundService
{
    protected readonly IConnection connection;
    protected readonly ILogger logger;

    protected IModel Model { get; private set; }

    public ushort PrefetchCount { get; }

    public string QueueName { get; }

    #region Constructors

    protected QueueServiceWorkerBase(ILogger logger, IConnection connection, string queueName, ushort prefetchCount)
    {
        this.logger = logger;
        this.connection = connection;
        QueueName = queueName;
        PrefetchCount = prefetchCount;
    }

    #endregion

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Model = BuildModel();

        IBasicConsumer consumer = BuildConsumer();

        WaitQueueCreation();

        string consumerTag = consumer.Model.BasicConsume(QueueName, false, consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogTrace("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }

        Model.BasicCancelNoWait(consumerTag);
    }

    protected void WaitQueueCreation()
    {
        Policy.Handle<OperationInterruptedException>()
              .WaitAndRetry(5, retryAttempt =>
                               {
                                   TimeSpan timeToWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                                   logger.LogWarning("Queue {queueName} not found... We will try in {tempo}.",
                                                     QueueName, timeToWait.Humanize());
                                   return timeToWait;
                               })
              .Execute(() =>
                       {
                           using IModel testModel = BuildModel();
                           testModel.QueueDeclarePassive(QueueName);
                       });
    }

    protected IModel BuildModel()
    {
        IModel model = connection.CreateModel();

        model.BasicQos(0, PrefetchCount, false);

        return model;
    }

    protected abstract IBasicConsumer BuildConsumer();
}