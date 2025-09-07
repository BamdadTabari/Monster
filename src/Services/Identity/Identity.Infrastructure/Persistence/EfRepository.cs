using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Identity.Infrastructure.Persistence;

public sealed class EfRepository<TEntity>(IdentityDbContext db) : IRepository<TEntity> where TEntity : class
{
    public Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken ct = default)
        => db.Set<TEntity>().FindAsync(keyValues, ct).AsTask();

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, bool asNoTracking = true, CancellationToken ct = default)
    {
        IQueryable<TEntity> q = db.Set<TEntity>();
        if (include is not null) q = include(q);
        if (asNoTracking) q = q.AsNoTracking();
        return q.FirstOrDefaultAsync(predicate, ct)!;
    }

    public Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, bool asNoTracking = true, CancellationToken ct = default)
    {
        IQueryable<TEntity> q = db.Set<TEntity>();
        if (include is not null) q = include(q);
        if (predicate is not null) q = q.Where(predicate);
        if (asNoTracking) q = q.AsNoTracking();
        return q.ToListAsync(ct);
    }

    public async Task<PageResponse<TEntity>> PageAsync(PageRequest page,
        Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = true, CancellationToken ct = default)
    {
        IQueryable<TEntity> q = db.Set<TEntity>();
        if (include is not null) q = include(q);
        if (predicate is not null) q = q.Where(predicate);
        var total = await q.LongCountAsync(ct);
        var items = await q.Skip(page.Skip).Take(page.Take).ToListAsync(ct);
        return new PageResponse<TEntity>(items, total, page.PageNumber, page.PageSize);
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => db.Set<TEntity>().AnyAsync(predicate, ct);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
        => predicate is null ? db.Set<TEntity>().CountAsync(ct) : db.Set<TEntity>().CountAsync(predicate, ct);

    public Task AddAsync(TEntity entity, CancellationToken ct = default) { db.Set<TEntity>().Add(entity); return Task.CompletedTask; }
    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) { db.Set<TEntity>().AddRange(entities); return Task.CompletedTask; }
    public void Update(TEntity entity) { db.Set<TEntity>().Update(entity); }
    public void Remove(TEntity entity) { db.Set<TEntity>().Remove(entity); }
}
