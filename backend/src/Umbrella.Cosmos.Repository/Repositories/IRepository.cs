using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Umbrella.Cosmos.Repository.Builders;

namespace Umbrella.Cosmos.Repository.Repositories;

public interface IRepository<T> where T : IDocument
{
    ValueTask<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

     ValueTask<T> CreateAsync(T value, CancellationToken cancellationToken = default);
     ValueTask<IEnumerable<T>> CreateAsync(IEnumerable<T> values, CancellationToken cancellationToken = default);

     ValueTask DeleteAsync(T value, CancellationToken cancellationToken = default);
     ValueTask DeleteAsync(string id, string? partitionKeyValue = null, CancellationToken cancellationToken = default);
     
     ValueTask<T?> TryGetAsync(
        string id,
        string? partitionKeyValue = null,
        CancellationToken cancellationToken = default);

    
    ValueTask<T> GetAsync(
        string id,
        string? partitionKeyValue = null,
        CancellationToken cancellationToken = default);

    
    ValueTask<T> GetAsync(
        string id,
        PartitionKey partitionKey,
        CancellationToken cancellationToken = default);

    
    ValueTask<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    
    ValueTask<IEnumerable<T>> GetByQueryAsync(
        string query,
        CancellationToken cancellationToken = default);

    ValueTask<T> UpdateAsync(T value, CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<T>> UpdateAsync(IEnumerable<T> values, CancellationToken cancellationToken = default);

    ValueTask UpdateAsync(string id,
        Action<DefaultPatchOperationBuilder<T>> builder,
        string? partitionKeyValue = null,
        string? etag = default,
        CancellationToken cancellationToken = default);


}