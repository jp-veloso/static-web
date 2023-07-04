using Umbrella.Cosmos.Repository.Options;

namespace Umbrella.Cosmos.Repository.Builders;

public class DefaultContainerOptionsBuilder
{
    private readonly List<ContainerOptions> _options = new();
    public IReadOnlyList<ContainerOptions> Options => _options;

    public DefaultContainerOptionsBuilder Configure<TItem>(Action<ContainerOptions> containerOptions) where TItem : IDocument
    {
        if (containerOptions is null) throw new ArgumentNullException(nameof(containerOptions));

        ContainerOptions optionsBuilder = new(typeof(TItem));

        containerOptions(optionsBuilder);

        _options.Add(optionsBuilder);

        return this;
    }
}