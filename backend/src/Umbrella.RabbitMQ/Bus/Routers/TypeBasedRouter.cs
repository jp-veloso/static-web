using System;
using System.Collections.Generic;

namespace Umbrella.RabbitMQ.Bus.Routers;

public class TypeBasedRouter : IRouteResolver
{
    private Dictionary<Type, Route> Routes { get; set; } = new();

    public Route ResolveRoute(IRouteable routeable)
    {
        if (routeable == null)
        {
            throw new ArgumentNullException(nameof(routeable));
        }

        Type type = routeable.GetType();

        return Routes.ContainsKey(type)
                   ? Routes[type]
                   : throw new InvalidOperationException($"Route not found for type {type}.");
    }

    public TypeBasedRouter AddRoute<T>(Route route)
    {
        if (route == null)
        {
            throw new ArgumentNullException(nameof(route));
        }

        Routes.Add(typeof(T), route);
        return this;
    }
}