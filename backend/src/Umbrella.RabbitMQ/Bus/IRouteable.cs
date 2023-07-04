using System.Collections.Generic;

namespace Umbrella.RabbitMQ.Bus;

public interface IRouteable
{
    Dictionary<string, object> Metadados { get; }
}