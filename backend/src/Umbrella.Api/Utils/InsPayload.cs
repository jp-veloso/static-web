using Umbrella.RabbitMQ.Bus;

namespace Umbrella.Api.Utils;

public class InsPayload : IEvent
{
    public List<ValueTuple<long, string>> Tuples { get; } = new();

    public Dictionary<string, object> Metadados { get; }

    public void AddValue(long insurerId, string insurerName)
    {
        Tuples.Add((insurerId, insurerName));
    }
}