using Monster.Persistence.Abstractions;

namespace Content.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);

    public Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => Task.FromResult<IUnitOfWorkTransaction>(new FakeTx());

    private sealed class FakeTx : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
