using System;
using System.Collections.Generic;

namespace Umbrella.RabbitMQ.Bus.Routers;

public class TypeAndFunctionBasedRouter : IRouteResolver
{
    private Dictionary<Type, Func<IRouteable, Route>> Routes { get; set; } = new();

    public Route ResolveRoute(IRouteable routeable)
    {
        if (routeable == null)
        {
            throw new ArgumentNullException(nameof(routeable));
        }

        Type type = routeable.GetType();
        Route route = default;
        if (Routes.ContainsKey(type))
        {
            route = Routes[type](routeable);
        }

        return route ?? throw new InvalidOperationException("Route not found");
    }

    public TypeAndFunctionBasedRouter AddRoute<T>(Func<IRouteable, Route> func)
    {
        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        Routes.Add(typeof(T), func);
        return this;
    }
}