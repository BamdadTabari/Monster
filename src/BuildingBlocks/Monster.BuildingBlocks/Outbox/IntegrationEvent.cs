namespace Monster.BuildingBlocks.Outbox;

/// <summary> Base type for integration events. </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredUtc { get; init; } = DateTime.UtcNow;
}
