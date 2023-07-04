using System;
using System.Collections.Generic;

namespace Umbrella.RabbitMQ.Bus.Routers;

public class FunctionBasedRouter : IRouteResolver
{
    private List<Func<IRouteable, Route>> Routes { get; set; } = new();

    public Route ResolveRoute(IRouteable routeable)
    {
        if (routeable == null)
        {
            throw new ArgumentNullException(nameof(routeable));
        }

        foreach (Func<IRouteable, Route> routeFunction in Routes)
        {
            Route route = routeFunction(routeable);
            if (route != null)
            {
                return route;
            }
        }

        throw new InvalidOperationException("Route not found");
    }

    public FunctionBasedRouter AddRoute(Func<IRouteable, Route> func)
    {
        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        Routes.Add(func);
        return this;
    }
}