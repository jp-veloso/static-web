using System.Linq.Expressions;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace Umbrella.Cosmos.Repository.Repositories;

public partial class DefaultRepository<T> : IRepository<T> where T : IDocument
{
    public async ValueTask<T?> TryGetAsync(string id, string? partitionKeyValue = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync(id, partitionKeyValue, cancellationToken);
        }
        catch (CosmosException e) when (e.StatusCode is HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public ValueTask<T> GetAsync(string id, string? partitionKeyValue = null, CancellationToken cancellationToken = default) =>
        GetAsync(id, new PartitionKey(partitionKeyValue ?? id), cancellationToken);

    public async ValueTask<T> GetAsync(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default)
    {
        Container container = await _provider.GetContainerAsync().ConfigureAwait(false);

        if (partitionKey == default)
        {
            partitionKey = new PartitionKey(id);
        }

        ItemResponse<T> response = await container.ReadItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        return response.Resource;
    }

    public async ValueTask<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        Container container = await _provider.GetContainerAsync().ConfigureAwait(false);

        IQueryable<T> query = container.GetItemLinqQueryable<T>(linqSerializerOptions: new CosmosLinqSerializerOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }).Where(predicate);

        (IEnumerable<T> items, var charge) = await IterateAsync(query, cancellationToken);
        
        return items;
    }

    public async ValueTask<IEnumerable<T>> GetByQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        Container container = await _provider.GetContainerAsync().ConfigureAwait(false);

        QueryDefinition queryDefinition = new(query);
        
        using FeedIterator<T> queryIterator = container.GetItemQueryIterator<T>(queryDefinition);
        
        List<T> results = new();

        while (queryIterator.HasMoreResults)
        {
            FeedResponse<T> response = await queryIterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            results.AddRange(response.Resource);
        }

        return results;
    }
}