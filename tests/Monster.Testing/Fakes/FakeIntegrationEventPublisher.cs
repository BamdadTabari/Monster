using Monster.BuildingBlocks.Outbox;

namespace Monster.Testing.Fakes;

public sealed class FakeIntegrationEventPublisher : IIntegrationEventPublisher
{
    public List<(IntegrationEvent Event, string? Topic, IDictionary<string,string>? Headers)> Published { get; } = new();

    public Task PublishAsync(IntegrationEvent @event, string? topic = null, IDictionary<string,string>? headers = null, CancellationToken ct = default)
    {
        Published.Add((@event, topic, headers is null ? null : new Dictionary<string,string>(headers)));
        return Task.CompletedTask;
    }
}
