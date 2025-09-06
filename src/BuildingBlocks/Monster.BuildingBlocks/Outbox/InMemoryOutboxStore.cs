using System.Collections.Concurrent;

namespace Monster.BuildingBlocks.Outbox;

/// <summary> DEV-ONLY in-memory outbox. Thread-safe enough for local/dev. </summary>
public sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _store = new();

    public Task AddAsync(OutboxMessage message, CancellationToken ct = default)
    {
        _store[message.Id] = message;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> PeekBatchAsync(int take, CancellationToken ct = default)
    {
        var batch = _store.Values
            .Where(m => m.DispatchedUtc is null)
            .OrderBy(m => m.OccurredUtc)
            .Take(take)
            .ToList()
            .AsReadOnly();
        return Task.FromResult((IReadOnlyList<OutboxMessage>)batch);
    }

    public Task MarkDispatchedAsync(Guid id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var m))
            _store[id] = m with { DispatchedUtc = DateTime.UtcNow };
        return Task.CompletedTask;
    }

    public Task IncrementAttemptAsync(Guid id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var m))
            _store[id] = m with { Attempt = m.Attempt + 1 };
        return Task.CompletedTask;
    }
}
