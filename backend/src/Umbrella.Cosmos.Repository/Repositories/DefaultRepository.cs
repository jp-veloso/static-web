using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Umbrella.Cosmos.Repository.Providers;

namespace Umbrella.Cosmos.Repository.Repositories;

public partial class DefaultRepository<T> : IRepository<T> where T : IDocument
{
    protected readonly DefaultCosmosContainerProvider<T> _provider;

    public DefaultRepository(DefaultCosmosContainerProvider<T> provider)
    {
        _provider = provider;
    }
    
    public async ValueTask<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        Container container = await _provider.GetContainerAsync().ConfigureAwait(false);

        IQueryable<T> query = container.GetItemLinqQueryable<T>().Where(predicate);

        return await query.CountAsync(cancellationToken);
    }
    private async ValueTask<(IEnumerable<T> items, double charge)> IterateAsync(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        using var iterator = queryable.ToFeedIterator();

        List<T> results = new();
        double charge = 0;

        while (iterator.HasMoreResults)
        {
            FeedResponse<T> feedResponse = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

            charge += feedResponse.RequestCharge;

            foreach (T result in feedResponse.Resource)
            {
                results.Add(result);
            }
        }

        return (results, charge);
    }

    static async Task<(List<T> items, double charge, string? continuationToken)> GetAllItemsAsync(IQueryable<T> query, int pageSize, CancellationToken cancellationToken = default)
    {
        string? continuationToken = null;
        List<T> results = new();
        var readItemsCount = 0;
        double charge = 0;
        using var iterator = query.ToFeedIterator();
        while (readItemsCount < pageSize && iterator.HasMoreResults)
        {
            FeedResponse<T> next = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

            foreach (T result in next)
            {
                if (readItemsCount == pageSize)
                {
                    break;
                }

                results.Add(result);
                readItemsCount++;
            }

            charge += next.RequestCharge;
            continuationToken = next.ContinuationToken;
        }

        return (results, charge, continuationToken);
    }
    
    
}