using Microsoft.Azure.Cosmos;
using Umbrella.Cosmos.Repository.Builders;

namespace Umbrella.Cosmos.Repository.Repositories;

public partial class DefaultRepository<T> : IRepository<T> where T : IDocument
{
    public async ValueTask<T> UpdateAsync(T value, CancellationToken cancellationToken = default)
    {
        Container container = await _provider.GetContainerAsync().ConfigureAwait(false);

        ItemResponse<T> response = await container.UpsertItemAsync(value, new PartitionKey(value.PartitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);

        return response.Resource;
    }
    
    public async ValueTask<IEnumerable<T>> UpdateAsync(IEnumerable<T> values, CancellationToken cancellationToken = default)
    {
        IEnumerable<Task<T>> updateTasks = values.Select(value => UpdateAsync(value, cancellationToken).AsTask()).ToList();

        await Task.WhenAll(updateTasks).ConfigureAwait(false);

        return updateTasks.Select(x => x.Result);
    }

    public async ValueTask UpdateAsync(string id,
        Action<DefaultPatchOperationBuilder<T>> builder,
        string? partitionKeyValue = null,
        string? etag = default,
        CancellationToken cancellationToken = default)
    {
        DefaultPatchOperationBuilder<T> patchOperationBuilder = new DefaultPatchOperationBuilder<T>(CosmosPropertyNamingPolicy.CamelCase);

        builder(patchOperationBuilder);

        Container container = await _provider.GetContainerAsync();

        partitionKeyValue ??= id;

        PatchItemRequestOptions patchItemRequestOptions = new();
        if (etag != default && !string.IsNullOrWhiteSpace(etag))
        {
            patchItemRequestOptions.IfMatchEtag = etag;
        }

        await container.PatchItemAsync<T>(id, new PartitionKey(partitionKeyValue), patchOperationBuilder.PatchOperations, patchItemRequestOptions, cancellationToken);
    }
    
}