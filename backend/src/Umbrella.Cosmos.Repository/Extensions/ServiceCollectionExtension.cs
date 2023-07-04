using Microsoft.Extensions.Configuration;
using Umbrella.Cosmos.Repository.Options;
using Umbrella.Cosmos.Repository.Providers;
using Umbrella.Cosmos.Repository.Repositories;
using Umbrella.Cosmos.Repository.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCosmosRepository(
        this IServiceCollection services,
        Action<RepositoryOptions> setupAction)
    {
        if (services is null)
        {
            throw new ArgumentNullException(
                nameof(services), "A service collection is required.");
        }
        
        services.AddOptions<RepositoryOptions>().Configure<IConfiguration>(
                (settings, configuration) => configuration.GetSection("cosmosRepository").Bind(settings));
        
        services
            .AddSingleton<DefaultCosmosClientOptionsProvider>()
            .AddSingleton<DefaultCosmosClientProvider>()
            .AddSingleton<DefaultCosmosContainerService>()
            .AddSingleton<IRepositoryFactory,DefaultRepositoryFactory>()
            .AddSingleton<DefaultItemConfigurationProvider>()
            .AddSingleton<DefaultItemInheritanceProvider>()
            .AddSingleton(typeof(IRepository<>), typeof(DefaultRepository<>))
            .AddSingleton(typeof(DefaultCosmosContainerProvider<>));

        services.PostConfigure(setupAction);

        return services;
    }
    
    
    
}