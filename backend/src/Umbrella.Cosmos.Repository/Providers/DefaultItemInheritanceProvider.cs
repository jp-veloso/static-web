using Microsoft.Extensions.Options;
using Umbrella.Cosmos.Repository.Options;

namespace Umbrella.Cosmos.Repository.Providers;

public class DefaultItemInheritanceProvider
{
    readonly RepositoryOptions _options;

    public DefaultItemInheritanceProvider(IOptions<RepositoryOptions> options)
    {
        _options = options.Value;
    }

    public Type GetDynamicType(string propertyName, params string[] discs)
    {
        var values = _options.InheritanceOptions.First(x => StringProcessor.ToSnake(x._memberInfo.Name) == propertyName);
        
        foreach (var (key, value) in values.Configurations)
        {
            if (key.Any(discs.Contains))
            {
                return value;
            }
        }

        throw new ArgumentNullException($"Discriminators [{string.Join(",", discs)}] has no type configured");
    }
}