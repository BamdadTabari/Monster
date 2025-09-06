namespace Monster.BuildingBlocks.Outbox;

public interface IOutboxStore
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxMessage>> PeekBatchAsync(int take, CancellationToken ct = default);
    Task MarkDispatchedAsync(Guid id, CancellationToken ct = default);
    Task IncrementAttemptAsync(Guid id, CancellationToken ct = default);
}
