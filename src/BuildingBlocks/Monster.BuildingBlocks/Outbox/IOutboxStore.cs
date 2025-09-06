namespace Monster.BuildingBlocks.Outbox;

public interface IOutboxStore
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);

    // Only return messages eligible to run now (NextAttemptUtc <= now)
    Task<IReadOnlyList<OutboxMessage>> PeekBatchAsync(int take, DateTime nowUtc, CancellationToken ct = default);

    Task MarkDispatchedAsync(Guid id, DateTime dispatchedUtc, CancellationToken ct = default);

    // On failure, bump Attempt and set next time
    Task IncrementAttemptAsync(Guid id, int newAttempt, DateTime nextAttemptUtc, CancellationToken ct = default);
}
