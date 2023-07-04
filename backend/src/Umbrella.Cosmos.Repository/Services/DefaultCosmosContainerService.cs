using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Umbrella.Cosmos.Repository.Options;
using Umbrella.Cosmos.Repository.Providers;

namespace Umbrella.Cosmos.Repository.Services;

public class DefaultCosmosContainerService
{
    private readonly DefaultCosmosClientProvider _clientProvider;
    private readonly DefaultItemConfigurationProvider _itemConfigurationProvider;
    private readonly RepositoryOptions _options;

    public DefaultCosmosContainerService(DefaultCosmosClientProvider clientProvider, DefaultItemConfigurationProvider itemConfigurationProvider, IOptions<RepositoryOptions> options)
    {
        _clientProvider = clientProvider;
        _itemConfigurationProvider = itemConfigurationProvider;
        _options = options.Value;
    }
    
    public Task<Container> GetContainerAsync<TItem>() where TItem : IDocument => GetContainerAsync(typeof(TItem));
    
    private async Task<Container> GetContainerAsync(Type itemType)
    {
        ItemConfiguration itemConfiguration = _itemConfigurationProvider.GetItemConfiguration(itemType);

        Database database = string.IsNullOrEmpty(itemConfiguration.InternalDatabase)
            ? await _clientProvider.UseClientAsync(client => Task.FromResult(client.GetDatabase(_options.DatabaseId))) :
              await _clientProvider.UseClientAsync(client => Task.FromResult(client.GetDatabase(itemConfiguration.InternalDatabase)));
            
        ContainerProperties containerProperties = new()
        {
            Id = itemConfiguration.ContainerName,
            PartitionKeyPath = itemConfiguration.PartitionKeyPath,
            UniqueKeyPolicy = itemConfiguration.UniqueKeyPolicy ?? new UniqueKeyPolicy(),
            DefaultTimeToLive = itemConfiguration.DefaultTimeToLive
        };

        Container container = await Task.FromResult(database.GetContainer(containerProperties.Id)).ConfigureAwait(false);

        //await container.ReplaceThroughputAsync(itemConfiguration.ThroughputProperties);
        //await container.ReplaceContainerAsync(containerProperties);

        return container;
    }
}