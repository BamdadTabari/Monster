using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks.Outbox;

namespace Identity.Infrastructure.Outbox;

public sealed class EfOutboxStore(IdentityDbContext db) : IOutboxStore
{
    public async Task AddAsync(OutboxMessage message, CancellationToken ct = default)
    {
        db.Set<OutboxMessage>().Add(message);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<OutboxMessage>> PeekBatchAsync(int take, DateTime nowUtc, CancellationToken ct = default)
    {
        return await db.Set<OutboxMessage>()
            .Where(m => m.DispatchedUtc == null && m.NextAttemptUtc <= nowUtc)
            .OrderBy(m => m.OccurredUtc).ThenBy(m => m.Attempt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task MarkDispatchedAsync(Guid id, DateTime dispatchedUtc, CancellationToken ct = default)
    {
        await db.Set<OutboxMessage>().Where(m => m.Id == id).ExecuteUpdateAsync(
            s => s.SetProperty(m => m.DispatchedUtc, dispatchedUtc), ct);
    }

    public async Task IncrementAttemptAsync(Guid id, int newAttempt, DateTime nextAttemptUtc, CancellationToken ct = default)
    {
        await db.Set<OutboxMessage>().Where(m => m.Id == id).ExecuteUpdateAsync(
            s => s.SetProperty(m => m.Attempt, newAttempt).SetProperty(m => m.NextAttemptUtc, nextAttemptUtc), ct);
    }
}
