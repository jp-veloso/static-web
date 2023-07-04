using Umbrella.Api.Services.Exceptions;

namespace Umbrella.Api.Resources.Exceptions;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext context, Exception ex)
    {
        switch (ex)
        {
            case ServiceException c:
                context.Response.StatusCode = c.StandardError.Status;
                c.StandardError.Path = context.Request.Path.ToUriComponent();
                return context.Response.WriteAsJsonAsync(c.StandardError);

            default:
                Console.WriteLine(ex);
                context.Response.StatusCode = 500;
                return context.Response.WriteAsync("Erro desconhecido");
        }
    }
}