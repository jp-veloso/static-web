using Microsoft.Azure.Cosmos;
using Umbrella.Cosmos.Repository.Services;

namespace Umbrella.Cosmos.Repository.Providers;

public class DefaultCosmosContainerProvider<T> where T : IDocument
{
    readonly Lazy<Task<Container>> _lazyContainer;

    public DefaultCosmosContainerProvider(DefaultCosmosContainerService containerService) =>
        _lazyContainer = new Lazy<Task<Container>>(async () => await containerService.GetContainerAsync<T>());

    public Task<Container> GetContainerAsync() => _lazyContainer.Value;
}