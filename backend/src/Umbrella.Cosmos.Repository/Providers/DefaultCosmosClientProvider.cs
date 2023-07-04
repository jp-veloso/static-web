using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Umbrella.Cosmos.Repository.Options;

namespace Umbrella.Cosmos.Repository.Providers;

public class DefaultCosmosClientProvider : IDisposable
{
    readonly Lazy<CosmosClient> _lazyCosmosClient;
    readonly CosmosClientOptions _cosmosClientOptions;
    readonly RepositoryOptions _options;

    private DefaultCosmosClientProvider(CosmosClientOptions cosmosClientOptions, IOptions<RepositoryOptions> options)
    {
        _cosmosClientOptions = cosmosClientOptions;
        _options = options.Value;
        _lazyCosmosClient = new Lazy<CosmosClient>(GetCosmoClient);
    }

    public DefaultCosmosClientProvider(DefaultCosmosClientOptionsProvider cosmosClientOptionsProvider,
        IOptions<RepositoryOptions> options) :
        this(cosmosClientOptionsProvider.ClientOptions, options)
    {
        _ = cosmosClientOptionsProvider ?? throw new ArgumentNullException(nameof(cosmosClientOptionsProvider), "Cosmos Client Options Provider is required.");
    }

    CosmosClient GetCosmoClient() => _options.TokenCredential is not null && _options.AccountEndpoint is not null
        ? new CosmosClient(_options.AccountEndpoint, _options.TokenCredential, _cosmosClientOptions)
        : new CosmosClient(_options.ConnectionString, _cosmosClientOptions);
    
    public Task<T> UseClientAsync<T>(Func<CosmosClient, Task<T>> consume) => consume.Invoke(_lazyCosmosClient.Value);

    public void Dispose()
    {
        if (_lazyCosmosClient.IsValueCreated)
        {
            _lazyCosmosClient.Value.Dispose();
        }
    }
}