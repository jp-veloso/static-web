using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Humanizer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Umbrella.RabbitMQ.Serialization;

namespace Umbrella.RabbitMQ.Rpc;

public class SimpleAmqpRpc
{
    private readonly ActivitySource activitySource;
    private readonly TimeSpan defaultTimeout;
    private readonly IModel model;
    private readonly IAmqpSerializer serializer;

    public SimpleAmqpRpc(IModel rabbitmqModel, IAmqpSerializer serializer, ActivitySource activitySource,
                         TimeSpan defaultTimeout)
    {
        model = rabbitmqModel;
        this.serializer = serializer;
        this.activitySource = activitySource;
        this.defaultTimeout = defaultTimeout;
    }

    public void FireAndForget<TRequest>(string exchangeName, string routingKey, TRequest requestModel)
    {
        Send(exchangeName, routingKey, requestModel);
    }

    public async Task<TResponse> SendAndReceiveAsync<TRequest, TResponse>(
        string exchangeName, string routingKey, TRequest requestModel, TimeSpan? receiveTimeout = null)
    {
        using Activity currentActivity = activitySource.SafeStartActivity("SimpleAmqpRpc.SendAndReceiveAsync");

        QueueDeclareOk queue = DeclareAnonymousQueue();

        currentActivity.SetTag("Request.ExchangeName", exchangeName);
        currentActivity.SetTag("Request.RoutingKey", routingKey);
        currentActivity.SetTag("Response.Queue", queue.QueueName);

        Task sendTask = Task.Run(() => { Send(exchangeName, routingKey, requestModel, queue.QueueName); });

        TResponse responseModel = default;

        Task receiveTask = Task.Run(() =>
                                    {
                                        responseModel = Receive<TResponse>(queue, receiveTimeout ?? defaultTimeout);
                                    });

        await Task.WhenAll(sendTask, receiveTask);

        currentActivity.SetEndTime(DateTime.UtcNow);

        return responseModel;
    }

    private QueueDeclareOk DeclareAnonymousQueue()
    {
        return model.QueueDeclare(string.Empty, false, true, true, null);
    }

    protected virtual void Send<TRequest>(string exchangeName, string routingKey, TRequest requestModel,
                                          string callbackQueueName = null)
    {
        using Activity currentActivity = activitySource.SafeStartActivity("SimpleAmqpRpc.Send", ActivityKind.Client);
        currentActivity.AddTag("Exchange", exchangeName);
        currentActivity.AddTag("RoutingKey", routingKey);
        currentActivity.AddTag("CallbackQueue", callbackQueueName);

        IBasicProperties requestProperties = model.CreateBasicProperties()
                                                  .SetTelemetry(currentActivity)
                                                  .SetMessageId()
                                                  .SetReplyTo(callbackQueueName);

        currentActivity.AddTag("MessageId", requestProperties.MessageId);
        currentActivity.AddTag("CorrelationId", requestProperties.CorrelationId);

        model.BasicPublish(exchangeName, routingKey, requestProperties,
                           serializer.Serialize(requestProperties, requestModel));

        currentActivity.SetEndTime(DateTime.UtcNow);
    }

    protected virtual TResponse Receive<TResponse>(QueueDeclareOk queue, TimeSpan receiveTimeout)
    {
        using BlockingCollection<AmqpResponse<TResponse>> localQueue = new();
        EventingBasicConsumer consumer = new(model);

        consumer.Received += (_, receivedItem) =>
                             {
                                 using Activity receiveActivity =
                                     activitySource.SafeStartActivity("SimpleAmqpRpc.Receive", ActivityKind.Server);
                                 receiveActivity.SetParentId(receivedItem.BasicProperties.GetTraceId(),
                                                             receivedItem.BasicProperties.GetSpanId(),
                                                             ActivityTraceFlags.Recorded);
                                 receiveActivity.AddTag("Queue", queue.QueueName);
                                 receiveActivity.AddTag("MessageId", receivedItem.BasicProperties.MessageId);
                                 receiveActivity.AddTag("CorrelationId", receivedItem.BasicProperties.CorrelationId);

                                 if (receivedItem.BasicProperties
                                                 .TryReconstructException(out AmqpRpcRemoteException exception))
                                 {
                                     localQueue.Add(new AmqpResponse<TResponse>(exception));
                                     localQueue.CompleteAdding();
                                 }
                                 else
                                 {
                                     TResponse result;
                                     try
                                     {
                                         result = serializer.Deserialize<TResponse>(receivedItem);
                                     }
                                     catch (Exception ex)
                                     {
                                         receiveActivity.SetStatus(ActivityStatusCode.Error);
                                         receiveActivity.AddEvent(new ActivityEvent("Erro na serialização ",
                                                                   tags: new ActivityTagsCollection
                                                                         {{"Exception", ex}}));
                                         throw;
                                     }

                                     localQueue.Add(new AmqpResponse<TResponse>(result));
                                     localQueue.CompleteAdding();
                                 }

                                 receiveActivity.SetEndTime(DateTime.UtcNow);
                             };

        string consumerTag = model.BasicConsume(queue.QueueName, true, consumer);
        AmqpResponse<TResponse> responseModel;
        try
        {
            if (!localQueue.TryTake(out responseModel, receiveTimeout))
            {
                throw new
                    TimeoutException($"The operation has timed-out after {receiveTimeout.Humanize()} waiting a RPC response at {queue.QueueName} queue.");
            }
        }
        finally
        {
            model.BasicCancelNoWait(consumerTag);
        }

        return responseModel.Exception != null ? throw responseModel.Exception : responseModel.Result;
    }
}