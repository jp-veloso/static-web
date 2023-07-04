using Umbrella.RabbitMQ.Bus;

namespace Umbrella.Api.Utils;

public class EnrollPayload : IEvent
{
    public List<ValueTuple<long, long, string>> Tuples { get; } = new();

    public Dictionary<string, object> Metadados { get; } = new();

    public void AddValue(long clientId, long insurerId, string cnpj)
    {
        Tuples.Add((clientId, insurerId, cnpj));
    }
}