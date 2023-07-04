namespace Umbrella.RabbitMQ.Consumer;

public enum PostConsumeAction
{
    None,
    Ack,
    Nack,
    Reject
}