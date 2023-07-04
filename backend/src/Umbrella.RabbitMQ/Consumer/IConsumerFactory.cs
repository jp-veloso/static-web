using RabbitMQ.Client;

namespace Umbrella.RabbitMQ.Consumer;

public interface IConsumerFactory
{
    IBasicConsumer BuildConsumer(IModel model);
}