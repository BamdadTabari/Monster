using System.Linq.Expressions;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Content.Application.Tests.Fakes;

public sealed class FakeRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _store = new();

    public Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken ct = default)
        => Task.FromResult<TEntity?>(null); // not needed here

    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = _store.AsQueryable();
        if (include is not null) q = include(q);
        return Task.FromResult(q.FirstOrDefault(predicate));
    }

    public Task<List<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = _store.AsQueryable();
        if (include is not null) q = include(q);
        if (predicate is not null) q = q.Where(predicate);
        return Task.FromResult(q.ToList());
    }

    public Task<PageResponse<TEntity>> PageAsync(
        PageRequest page,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> q = _store.AsQueryable();
        if (include is not null) q = include(q);
        if (predicate is not null) q = q.Where(predicate);
        var total = q.LongCount();
        var items = q.Skip(page.Skip).Take(page.Take).ToList();
        return Task.FromResult(new PageResponse<TEntity>(items, total, page.PageNumber, page.PageSize));
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => Task.FromResult(_store.AsQueryable().Any(predicate));

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
        => Task.FromResult(predicate is null ? _store.Count : _store.AsQueryable().Count(predicate));

    public Task AddAsync(TEntity entity, CancellationToken ct = default) { _store.Add(entity); return Task.CompletedTask; }
    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) { _store.AddRange(entities); return Task.CompletedTask; }
    public void Update(TEntity entity) { /* no-op for fake */ }
    public void Remove(TEntity entity) { _store.Remove(entity); }

    // helper for seeding
    public void Seed(params TEntity[] entities) => _store.AddRange(entities);
}
