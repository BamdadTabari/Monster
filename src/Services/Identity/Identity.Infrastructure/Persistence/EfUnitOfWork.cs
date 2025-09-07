using Microsoft.EntityFrameworkCore.Storage;
using Monster.Persistence.Abstractions;

namespace Identity.Infrastructure.Persistence;

public sealed class EfUnitOfWork(IdentityDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var tx = await db.Database.BeginTransactionAsync(ct);
        return new EfTx(tx);
    }

    private sealed class EfTx(IDbContextTransaction inner) : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken ct = default) => inner.CommitAsync(ct);
        public Task RollbackAsync(CancellationToken ct = default) => inner.RollbackAsync(ct);
        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
