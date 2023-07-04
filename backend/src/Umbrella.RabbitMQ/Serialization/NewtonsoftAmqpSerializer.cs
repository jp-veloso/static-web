using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Umbrella.RabbitMQ.Serialization;

public class NewtonsoftAmqpSerializer : AmqpBaseSerializer
{
    public NewtonsoftAmqpSerializer(ActivitySource activitySource) : base(activitySource, "NewtonsoftAmqpSerializer")
    {
    }

    protected override TResponse DeserializeInternal<TResponse>(IBasicProperties basicProperties,
                                                                ReadOnlyMemory<byte> body)
    {
        string message = Encoding.UTF8.GetString(body.ToArray());
        return JsonConvert.DeserializeObject<TResponse>(message);
    }

    protected override byte[] SerializeInternal<T>(IBasicProperties basicProperties, T objectToSerialize)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objectToSerialize));
    }
}