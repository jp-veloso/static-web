using Umbrella.Api.Resources.Exceptions;

namespace Umbrella.Api.Services.Exceptions;

public class ServiceException : Exception
{
    public StandardError StandardError { get; }

    public ServiceException(string msg, StandardError standardError) : base(msg)
    {
        StandardError = standardError;
    }
}