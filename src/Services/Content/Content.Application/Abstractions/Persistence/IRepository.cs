using System.Linq.Expressions;
using Monster.BuildingBlocks;

namespace Content.Application.Abstractions.Persistence;

/// <summary>
/// Generic repository abstraction for aggregate roots/entities.
/// Keep EF-specific details out of Application. Supports includes, paging, AsNoTracking.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    // Lookup
    Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken ct = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<List<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<PageResponse<TEntity>> PageAsync(
        PageRequest page,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);

    // Commands
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
