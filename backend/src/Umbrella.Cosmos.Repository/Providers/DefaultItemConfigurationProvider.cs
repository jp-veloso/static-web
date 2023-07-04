using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Options;
using Umbrella.Cosmos.Repository.Options;

namespace Umbrella.Cosmos.Repository.Providers;

public class DefaultItemConfigurationProvider
{
    private readonly RepositoryOptions _options;
    private static readonly ConcurrentDictionary<Type, ItemConfiguration> _itemOptionsMap = new();

    public DefaultItemConfigurationProvider(IOptions<RepositoryOptions> options)
    {
        _options = options.Value;
    }
    
    public ItemConfiguration GetItemConfiguration<TItem>() where TItem : IDocument =>
        GetItemConfiguration(typeof(TItem));
    public ItemConfiguration GetItemConfiguration(Type itemType) =>
        _itemOptionsMap.GetOrAdd(itemType, AddOption(itemType));
    private ItemConfiguration AddOption(Type type)
    {
        ContainerOptions containerOptions = _options.GetContainerOptions(type) ?? throw new ArgumentNullException("Not found container configuration for type: " + type.Name);

        return new ItemConfiguration(
            containerOptions.Type,
            containerOptions.Collection!,
            containerOptions.PartitionKey ?? "/id",
            null,
            containerOptions.ThroughputProperties,
            containerOptions.InternalDatabase,
            (int)(containerOptions.TimeToLive?.TotalSeconds ?? -1)
            );

    }
}