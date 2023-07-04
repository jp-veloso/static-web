using Azure.Core;
using Umbrella.Cosmos.Repository.Builders;

namespace Umbrella.Cosmos.Repository.Options;

public class RepositoryOptions
{
    public string? ConnectionString { get; set; }

    public string? AccountEndpoint { get; set; }

    public string DatabaseId { get; set; } = "database";
    
    public TokenCredential? TokenCredential { get; set; } = null;
    
    public DefaultInheritanceOptionsBuilder InheritanceOptionsBuilder { get; } = new();

    internal IReadOnlyList<InheritanceOptions> InheritanceOptions => InheritanceOptionsBuilder.Options;
    
    public DefaultContainerOptionsBuilder ContainerBuilder { get; } = new();
    
    internal IReadOnlyList<ContainerOptions> Options => ContainerBuilder.Options;
    
    internal ContainerOptions? GetContainerOptions<TItem>() where TItem : IDocument =>
        GetContainerOptions(typeof(TItem));

    internal ContainerOptions? GetContainerOptions(Type itemType) =>
        Options.FirstOrDefault(co => co.Type == itemType);
}