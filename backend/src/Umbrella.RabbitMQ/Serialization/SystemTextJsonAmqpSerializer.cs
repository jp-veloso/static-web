using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Umbrella.RabbitMQ.Serialization;

public class SystemTextJsonAmqpSerializer : AmqpBaseSerializer
{
    public SystemTextJsonAmqpSerializer(ActivitySource activitySource) : base(activitySource,
                                                                              "SystemTextJsonAmqpSerializer")
    {
    }

    protected override TResponse DeserializeInternal<TResponse>(IBasicProperties basicProperties,
                                                                ReadOnlyMemory<byte> body)
    {
        string message = Encoding.UTF8.GetString(body.ToArray());
        return JsonSerializer.Deserialize<TResponse>(message);
    }

    protected override byte[] SerializeInternal<T>(IBasicProperties basicProperties, T objectToSerialize)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(objectToSerialize));
    }
}