using System.Linq.Expressions;
using System.Reflection;

namespace Umbrella.Api.Utils.Pagination;

public static class Paginator
{
    public static Expression AddOrderBy(Expression source, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }

        string[] orders = orderBy.Split(",", StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < orders.Length; i++)
        {
            source = GenerateOrderExpression(source, orders[i], i);
        }

        return source;
    }

    private static Expression GenerateOrderExpression(Expression source, string orderBy, int index)
    {
        string[] orderByParams = orderBy.Trim()
                                        .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

        string orderByMethodName = index == 0 ? "OrderBy" : "ThenBy";

        string parameterPath = orderByParams[0];

        if (orderByParams.Length > 1 && orderByParams[1]
               .Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            orderByMethodName += "Descending";
        }

        Type sourceType = source.Type.GetGenericArguments()
                                .First();

        ParameterExpression parameterExpression = Expression.Parameter(sourceType, "p");

        Expression orderByExpression = BuildPropertyPathExpression(parameterExpression, parameterPath);

        Type orderByFuncType = typeof(Func<,>).MakeGenericType(sourceType, orderByExpression.Type);

        LambdaExpression orderByLambda = Expression.Lambda(orderByFuncType, orderByExpression, parameterExpression);

        source = Expression.Call(typeof(Queryable), orderByMethodName, new[] {sourceType, orderByExpression.Type},
                                 source, orderByLambda);
        return source;
    }

    private static Expression BuildPropertyPathExpression(Expression rootExpression, string propertyPath)
    {
        while (true)
        {
            string[] parts = propertyPath.Split(new[] {'.'}, 2);

            string currentProperty = parts[0];

            PropertyInfo? propertyDescription =
                rootExpression.Type.GetProperty(currentProperty,
                                                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

            if (propertyDescription == null)
            {
                throw new KeyNotFoundException($"Cannot find property {rootExpression.Type.Name}.{currentProperty}");
            }

            MemberExpression propExpr = Expression.Property(rootExpression, propertyDescription);

            if (parts.Length <= 1)
            {
                return propExpr;
            }

            rootExpression = propExpr;
            propertyPath = parts[1];
        }
    }
}