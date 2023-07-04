using System.Collections.Generic;

namespace Umbrella.RabbitMQ.Bus.Routers;

public class Route
{
    public string ExchangeName { get; set; }

    public string RoutingKey { get; set; }

    public virtual void ConfigureHeaders(IDictionary<string, object> headers)
    {
        // deve ser extendido se necessário para conseguir alterar os headers do 
    }
}