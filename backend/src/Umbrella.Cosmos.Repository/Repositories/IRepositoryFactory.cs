namespace Umbrella.Cosmos.Repository.Repositories;

public interface IRepositoryFactory
{
    IRepository<T> RepositoryOf<T>() where T : class, IDocument;
}