using Microsoft.Azure.Cosmos;

namespace Umbrella.Cosmos.Repository.Repositories;

public partial class DefaultRepository<T> : IRepository<T> where T : IDocument
{
    public async ValueTask<T> CreateAsync(T value, CancellationToken cancellationToken = default)
    {
        Container container = await _provider.GetContainerAsync().ConfigureAwait(false);

        ItemResponse<T> response = await container.CreateItemAsync(value, new PartitionKey(value.PartitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);

        return response.Resource;
    }
    
    public async ValueTask<IEnumerable<T>> CreateAsync(IEnumerable<T> values, CancellationToken cancellationToken = default)
    {
        IEnumerable<Task<T>> creationTasks = values.Select(value => CreateAsync(value, cancellationToken).AsTask()).ToList();

        _ = await Task.WhenAll(creationTasks).ConfigureAwait(false);

        return creationTasks.Select(x => x.Result);
    }
}