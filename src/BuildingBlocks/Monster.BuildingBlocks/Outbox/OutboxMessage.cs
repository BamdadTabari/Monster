namespace Monster.BuildingBlocks.Outbox;

public sealed record OutboxMessage(
    Guid Id,
    string Type,              // e.g., "CategoryCreated"
    string Topic,             // e.g., "content.category.created"
    string Payload,           // JSON
    DateTime OccurredUtc,
    DateTime? DispatchedUtc,
    int Attempt,
    DateTime NextAttemptUtc
);
