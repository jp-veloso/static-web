using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Umbrella.Cosmos.Repository.Serializers;

namespace Umbrella.Cosmos.Repository.Providers;

public class DefaultCosmosClientOptionsProvider
{
    readonly Lazy<CosmosClientOptions> _lazyClientOptions;
    public CosmosClientOptions ClientOptions => _lazyClientOptions.Value;

    public DefaultCosmosClientOptionsProvider(
        IServiceProvider serviceProvider, 
        DefaultItemInheritanceProvider itemInheritanceProvider)
    {
        _lazyClientOptions = new Lazy<CosmosClientOptions>(() => CreateCosmosClientOptions(serviceProvider, itemInheritanceProvider));
    }
    
    CosmosClientOptions CreateCosmosClientOptions(IServiceProvider serviceProvider, DefaultItemInheritanceProvider itemInheritanceProvider)
    {
        CosmosClientOptions options = new CosmosClientOptions()
        {
            Serializer = new DefaultCosmosSerializer(CreateSerializer(itemInheritanceProvider)),
        };

        return options;
    }

    private JsonSerializerSettings CreateSerializer(DefaultItemInheritanceProvider provider)
    {
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new CustomJsonConverter(provider),
                new StringEnumConverter()
            }
        };

        return jsonSerializerSettings;
    }
}