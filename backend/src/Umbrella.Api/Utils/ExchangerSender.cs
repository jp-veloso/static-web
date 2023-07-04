using Umbrella.RabbitMQ.Bus;

namespace Umbrella.Api.Utils;

public class ExchangerSender
{
    private readonly IEventBus _bus;

    public ExchangerSender(IEventBus bus)
    {
        _bus = bus;
    }

    public void SendToRabbit(IEvent payload)
    {
        _bus.PublishEvent(payload);
    }
}