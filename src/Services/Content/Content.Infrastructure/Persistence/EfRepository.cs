using System.Linq.Expressions;
using Content.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;

namespace Content.Infrastructure.Persistence;

/// <summary>
/// EF Core generic repository. Correctly applies AsNoTracking when requested,
/// supports includes, paging, and cancellation.
/// </summary>
public sealed class EfRepository<TEntity> : Application.Abstractions.Persistence.IRepository<TEntity> where TEntity : class
{
    private readonly ContentDbContext _db;

    public EfRepository(ContentDbContext db) => _db = db;

    private IQueryable<TEntity> Query(bool asNoTracking)
    {
        var q = _db.Set<TEntity>().AsQueryable();
        if (asNoTracking) q = q.AsNoTracking();
        return q;
    }

    public async Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken ct = default)
        => await _db.Set<TEntity>().FindAsync(keyValues, ct).AsTask();

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        var q = Query(asNoTracking);
        if (include is not null) q = include(q);
        return await q.FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<List<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        var q = Query(asNoTracking);
        if (include is not null) q = include(q);
        if (predicate is not null) q = q.Where(predicate);
        return await q.ToListAsync(ct);
    }

    public async Task<PageResponse<TEntity>> PageAsync(
        PageRequest page,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        var q = Query(asNoTracking);
        if (include is not null) q = include(q);
        if (predicate is not null) q = q.Where(predicate);

        var total = await q.LongCountAsync(ct);
        var items = await q.Skip(page.Skip).Take(page.Take).ToListAsync(ct);
        return new PageResponse<TEntity>(items, total, page.PageNumber, page.PageSize);
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => Query(asNoTracking: true).AnyAsync(predicate, ct);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
        => predicate is null
            ? Query(asNoTracking: true).CountAsync(ct)
            : Query(asNoTracking: true).CountAsync(predicate, ct);

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        _db.Set<TEntity>().Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        _db.Set<TEntity>().AddRange(entities);
        return Task.CompletedTask;
    }

    public void Update(TEntity entity) => _db.Set<TEntity>().Update(entity);
    public void Remove(TEntity entity) => _db.Set<TEntity>().Remove(entity);
}
