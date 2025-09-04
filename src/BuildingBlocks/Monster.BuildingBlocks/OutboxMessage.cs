namespace Monster.BuildingBlocks;

/// <summary>Reliable event storage to publish later (Outbox pattern).</summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public string Type { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
