namespace Monster.BuildingBlocks.Messaging;

public interface IMessageBusPublisher
{
    Task PublishAsync(string topic, ReadOnlyMemory<byte> body, IDictionary<string, string>? headers = null, CancellationToken ct = default);
}
