using System.Collections.Concurrent;

namespace Monster.BuildingBlocks.Outbox;

public sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _store = new();

    public Task AddAsync(OutboxMessage message, CancellationToken ct = default)
    {
        _store[message.Id] = message;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> PeekBatchAsync(int take, DateTime nowUtc, CancellationToken ct = default)
    {
        var batch = _store.Values
            .Where(m => m.DispatchedUtc is null && m.NextAttemptUtc <= nowUtc)
            .OrderBy(m => m.OccurredUtc)
            .ThenBy(m => m.Attempt)
            .Take(take)
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<OutboxMessage>)batch);
    }

    public Task MarkDispatchedAsync(Guid id, DateTime dispatchedUtc, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var m))
            _store[id] = m with { DispatchedUtc = dispatchedUtc };
        return Task.CompletedTask;
    }

    public Task IncrementAttemptAsync(Guid id, int newAttempt, DateTime nextAttemptUtc, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var m))
            _store[id] = m with { Attempt = newAttempt, NextAttemptUtc = nextAttemptUtc };
        return Task.CompletedTask;
    }
}
