using System.Linq.Expressions;

namespace Umbrella.Api.Utils.Pagination;

public static class Extensions
{
    public static Page<T> ToPaged<T>(this IQueryable<T> source, Pageable pageable) where T : class
    {
        Expression queryExpression = Paginator.AddOrderBy(source.Expression, pageable.Sort);

        if (queryExpression.CanReduce)
        {
            queryExpression = queryExpression.Reduce();
        }

        source = source.Provider.CreateQuery<T>(queryExpression);

        int total = source.Count();

        Page<T> results = new(source.Skip((pageable.Page - 1) * pageable.Size)
                                    .Take(pageable.Size)
                                    .ToList())
                          {
                              PageNumber = pageable.Page,
                              PageSize = pageable.Size,
                              TotalElements = total,
                              TotalPages = (int) Math.Ceiling((double) total / pageable.Size)
                          };

        return results;
    }
}