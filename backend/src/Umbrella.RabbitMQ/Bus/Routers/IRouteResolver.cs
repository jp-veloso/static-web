namespace Umbrella.RabbitMQ.Bus.Routers;

public interface IRouteResolver
{
    Route ResolveRoute(IRouteable routeable);
}