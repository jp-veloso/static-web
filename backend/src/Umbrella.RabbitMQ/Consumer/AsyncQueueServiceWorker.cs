using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Umbrella.RabbitMQ.Serialization;

namespace Umbrella.RabbitMQ.Consumer;

public class AsyncQueueServiceWorker<TRequest, TResponse> : QueueServiceWorkerBase where TResponse : Task
{
    protected readonly ActivitySource activitySource;
    protected readonly Func<TRequest, TResponse> dispatchFunc;

    protected readonly IAmqpSerializer serializer;

    #region Constructors

    public AsyncQueueServiceWorker(ILogger logger, IConnection connection, IAmqpSerializer serializer,
                                   ActivitySource activitySource, string queueName, ushort prefetchCount,
                                   Func<TRequest, TResponse> dispatchFunc) : base(logger, connection, queueName,
     prefetchCount)
    {
        this.serializer = serializer;
        this.activitySource = activitySource;
        this.dispatchFunc = dispatchFunc;
    }

    #endregion

    protected override IBasicConsumer BuildConsumer()
    {
        AsyncEventingBasicConsumer consumer = new(Model);

        consumer.Received += Receive;

        return consumer;
    }

    private async Task Receive(object sender, BasicDeliverEventArgs receivedItem)
    {
        if (receivedItem == null)
        {
            throw new ArgumentNullException(nameof(receivedItem));
        }

        if (receivedItem.BasicProperties == null)
        {
            throw new ArgumentNullException(nameof(receivedItem.BasicProperties));
        }

        using Activity receiveActivity =
            activitySource.SafeStartActivity("AsyncQueueServiceWorker.Receive", ActivityKind.Server);
        receiveActivity?.AddTag("Queue", QueueName);
        receiveActivity?.AddTag("MessageId", receivedItem.BasicProperties.MessageId);
        receiveActivity?.AddTag("CorrelationId", receivedItem.BasicProperties.CorrelationId);

        PostConsumeAction postReceiveAction = TryDeserialize(receivedItem, out TRequest request);

        if (postReceiveAction == PostConsumeAction.None)
        {
            try
            {
                postReceiveAction = await Dispatch(receivedItem, receiveActivity, request);
            }
            catch (Exception exception)
            {
                postReceiveAction = PostConsumeAction.Nack;
                logger.LogWarning("Exception on processing message {queueName} {exception}", QueueName, exception);
            }
        }

        switch (postReceiveAction)
        {
            case PostConsumeAction.None: throw new InvalidOperationException("None is unsupported");
            case PostConsumeAction.Ack:
                Model.BasicAck(receivedItem.DeliveryTag, false);
                break;
            case PostConsumeAction.Nack:
                Model.BasicNack(receivedItem.DeliveryTag, false, false);
                break;
            case PostConsumeAction.Reject:
                Model.BasicReject(receivedItem.DeliveryTag, false);
                break;
        }

        receiveActivity?.SetEndTime(DateTime.UtcNow);
    }

    private PostConsumeAction TryDeserialize(BasicDeliverEventArgs receivedItem, out TRequest request)
    {
        if (receivedItem is null)
        {
            throw new ArgumentNullException(nameof(receivedItem));
        }

        PostConsumeAction postReceiveAction = PostConsumeAction.None;

        request = default;
        try
        {
            request = serializer.Deserialize<TRequest>(receivedItem);
        }
        catch (Exception exception)
        {
            postReceiveAction = PostConsumeAction.Reject;

            logger.LogWarning("Message rejected during desserialization {exception}", exception);
        }

        return postReceiveAction;
    }

    protected virtual async Task<PostConsumeAction> Dispatch(BasicDeliverEventArgs receivedItem,
                                                             Activity receiveActivity, TRequest request)
    {
        if (receivedItem is null)
        {
            throw new ArgumentNullException(nameof(receivedItem));
        }

        using Activity dispatchActivity =
            activitySource.SafeStartActivity("AsyncQueueServiceWorker.Dispatch", ActivityKind.Internal,
                                             receiveActivity.Context);

        await dispatchFunc(request);

        dispatchActivity?.SetEndTime(DateTime.UtcNow);

        return PostConsumeAction.Ack;
    }
}