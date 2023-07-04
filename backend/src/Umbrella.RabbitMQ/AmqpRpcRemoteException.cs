using System;
using System.Runtime.Serialization;

namespace Umbrella.RabbitMQ;

[Serializable]
public class AmqpRpcRemoteException : Exception
{
    public override string StackTrace { get; }

    public AmqpRpcRemoteException() : this(null, null, null)
    {
    }

    public AmqpRpcRemoteException(string message, string remoteStackTrace, Exception inner) : base(message, inner)
    {
        StackTrace = remoteStackTrace;
    }

    protected AmqpRpcRemoteException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}