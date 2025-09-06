namespace Monster.BuildingBlocks.Outbox;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationEvent @event, string? topic = null, IDictionary<string,string>? headers = null, CancellationToken ct = default);
}
