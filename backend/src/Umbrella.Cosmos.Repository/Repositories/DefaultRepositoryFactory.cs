using Microsoft.Extensions.DependencyInjection;

namespace Umbrella.Cosmos.Repository.Repositories;

public class DefaultRepositoryFactory : IRepositoryFactory
{
    readonly IServiceProvider _serviceProvider;

    public DefaultRepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IRepository<T> RepositoryOf<T>() where T : class, IDocument =>
        _serviceProvider.GetRequiredService<IRepository<T>>();
    
}