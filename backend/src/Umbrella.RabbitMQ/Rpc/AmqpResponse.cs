using System;

namespace Umbrella.RabbitMQ.Rpc;

public class AmqpResponse<T>
{
    public AmqpRpcRemoteException Exception { get; set; }
    public T Result { get; set; }

    public AmqpResponse(AmqpRpcRemoteException exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public AmqpResponse(T result)
    {
        Result = result;
    }
}