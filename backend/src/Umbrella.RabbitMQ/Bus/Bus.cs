using System;
using System.Diagnostics;
using RabbitMQ.Client;
using Umbrella.RabbitMQ.Buffers;
using Umbrella.RabbitMQ.Bus.Routers;
using Umbrella.RabbitMQ.Serialization;

namespace Umbrella.RabbitMQ.Bus;

public class Bus : IEventBus, ICommandBus
{
    private readonly ActivitySource activitySource;
    private readonly RingBuffer<IModel> modelBuffer;
    private readonly IRouteResolver routeResolver;
    private readonly IAmqpSerializer serializer;

    public Bus(RingBuffer<IModel> modelBuffer, IAmqpSerializer serializer, ActivitySource activitySource,
               IRouteResolver routeResolver)
    {
        this.modelBuffer = modelBuffer;
        this.serializer = serializer;
        this.activitySource = activitySource;
        this.routeResolver = routeResolver;
    }

    public void SendCommand<TCommand>(TCommand command) where TCommand : class, ICommand
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Send(routeResolver.ResolveRoute(command), command);
    }

    public void PublishEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        Send(routeResolver.ResolveRoute(@event), @event);
    }

    protected virtual void Send<TRequest>(Route route, TRequest requestModel)
    {
        if (route == null)
        {
            throw new ArgumentNullException(nameof(route));
        }

        if (requestModel == null)
        {
            throw new ArgumentNullException(nameof(requestModel));
        }

        using Activity currentActivity =
            activitySource.SafeStartActivity($"{nameof(Bus)}.{nameof(Send)}", ActivityKind.Client);
        currentActivity.AddTag("Exchange", route.ExchangeName);
        currentActivity.AddTag("RoutingKey", route.RoutingKey);

        using IAccquisitonController<IModel> modelBuffered = modelBuffer.Accquire();

        IBasicProperties requestProperties = modelBuffered.Instance.CreateBasicProperties()
                                                          .SetTelemetry(currentActivity)
                                                          .SetMessageId();

        route.ConfigureHeaders(requestProperties.Headers);

        currentActivity.AddTag("MessageId", requestProperties.MessageId);
        currentActivity.AddTag("CorrelationId", requestProperties.CorrelationId);

        modelBuffered.Instance.BasicPublish(route.ExchangeName, route.RoutingKey, requestProperties,
                                            serializer.Serialize(requestProperties, requestModel));

        currentActivity.SetEndTime(DateTime.UtcNow);
    }
}