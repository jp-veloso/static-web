using System;
using System.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Umbrella.RabbitMQ.Serialization;

public abstract class AmqpBaseSerializer : IAmqpSerializer
{
    private readonly ActivitySource activitySource;
    private readonly string name;

    public AmqpBaseSerializer(ActivitySource activitySource, string name)
    {
        this.activitySource = activitySource;
        this.name = name;
    }

    public TResponse Deserialize<TResponse>(BasicDeliverEventArgs eventArgs)
    {
        if (eventArgs is null)
        {
            throw new ArgumentNullException(nameof(eventArgs));
        }

        if (eventArgs.BasicProperties is null)
        {
            throw new ArgumentNullException(nameof(eventArgs.BasicProperties));
        }

        using Activity receiveActivity = activitySource.SafeStartActivity($"{name}.Deserialize");
        TResponse returnValue;
        try
        {
            returnValue = DeserializeInternal<TResponse>(eventArgs.BasicProperties, eventArgs.Body);
        }
        catch (Exception ex)
        {
            receiveActivity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            throw;
        }
        finally
        {
            receiveActivity?.SetEndTime(DateTime.UtcNow);
        }

        return returnValue;
    }

    public byte[] Serialize<T>(IBasicProperties basicProperties, T objectToSerialize)
    {
        if (basicProperties is null)
        {
            throw new ArgumentNullException(nameof(basicProperties));
        }

        using Activity receiveActivity = activitySource.SafeStartActivity($"{name}.Serialize");
        byte[] returnValue;
        try
        {
            returnValue = SerializeInternal(basicProperties, objectToSerialize);
        }
        catch (Exception ex)
        {
            receiveActivity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            throw;
        }
        finally
        {
            receiveActivity?.SetEndTime(DateTime.UtcNow);
        }

        return returnValue;
    }

    protected abstract TResponse DeserializeInternal<TResponse>(IBasicProperties basicProperties,
                                                                ReadOnlyMemory<byte> body);

    protected abstract byte[] SerializeInternal<T>(IBasicProperties basicProperties, T objectToSerialize);
}