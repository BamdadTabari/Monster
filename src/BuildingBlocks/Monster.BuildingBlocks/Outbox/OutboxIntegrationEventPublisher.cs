using System.Text.Json;
using Monster.BuildingBlocks;

namespace Monster.BuildingBlocks.Outbox;

public sealed class OutboxIntegrationEventPublisher(IOutboxStore store, IDateTimeProvider clock) : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync(IntegrationEvent @event, string? topic = null, IDictionary<string,string>? headers = null, CancellationToken ct = default)
    {
        var type = @event.GetType().Name;
        var resolvedTopic = topic ?? ToTopic(type);
        var payload = JsonSerializer.Serialize(@event, @event.GetType(), JsonOpts);
        var now = clock.UtcNow();

        var message = new OutboxMessage(
            Id: Guid.NewGuid(),
            Type: type,
            Topic: resolvedTopic,
            Payload: payload,
            OccurredUtc: now,
            DispatchedUtc: null,
            Attempt: 0,
            NextAttemptUtc: now // ready to send immediately
        );

        await store.AddAsync(message, ct);
    }

    private static string ToTopic(string typeName)
    {
        // "CategoryCreated" -> "category.created"
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        foreach (var ch in typeName)
        {
            if (char.IsUpper(ch) && current.Length > 0)
            {
                parts.Add(current.ToString().ToLowerInvariant());
                current.Clear();
            }
            current.Append(ch);
        }
        if (current.Length > 0) parts.Add(current.ToString().ToLowerInvariant());
        return string.Join('.', parts);
    }
}
