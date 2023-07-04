using Microsoft.Azure.Cosmos;

namespace Umbrella.Cosmos.Repository.Repositories;

public partial class DefaultRepository<T> : IRepository<T> where T : IDocument
{
    public ValueTask DeleteAsync(T value, CancellationToken cancellationToken = default) => DeleteAsync(value.Id, value.PartitionKey, cancellationToken);

    public ValueTask DeleteAsync(string id, string? partitionKeyValue = null, CancellationToken cancellationToken = default) => DeleteAsync(id, new PartitionKey(partitionKeyValue ?? id), cancellationToken);
    
    public async ValueTask DeleteAsync(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default)
    {
        Container container = await _provider.GetContainerAsync().ConfigureAwait(false);

        if (partitionKey == default)
        {
            partitionKey = new PartitionKey(id);
        }

        _ = await container.DeleteItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    
}