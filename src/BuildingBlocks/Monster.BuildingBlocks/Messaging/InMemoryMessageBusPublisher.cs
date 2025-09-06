using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Monster.BuildingBlocks.Messaging;

/// <summary>
/// DEV-ONLY bus that just logs and stores messages for inspection/tests.
/// Swap with Rabbit/Kafka later via DI.
/// </summary>
public sealed class InMemoryMessageBusPublisher(ILogger<InMemoryMessageBusPublisher> logger) : IMessageBusPublisher
{
    public static ConcurrentBag<(string Topic, byte[] Body, IDictionary<string,string>? Headers)> Published { get; } = new();

    public Task PublishAsync(string topic, ReadOnlyMemory<byte> body, IDictionary<string, string>? headers = null, CancellationToken ct = default)
    {
        logger.LogInformation("Bus: published to {Topic} ({Length} bytes)", topic, body.Length);
        Published.Add((topic, body.ToArray(), headers is null ? null : new Dictionary<string,string>(headers)));
        return Task.CompletedTask;
    }
}
