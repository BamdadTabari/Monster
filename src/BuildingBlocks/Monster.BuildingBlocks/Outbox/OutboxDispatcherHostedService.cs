using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monster.BuildingBlocks.Messaging;

namespace Monster.BuildingBlocks.Outbox;

/// <summary>
/// Periodically drains the outbox and publishes messages to the bus. DEV-ONLY in same process.
/// </summary>
public sealed class OutboxDispatcherHostedService(
    IOutboxStore store,
    IMessageBusPublisher bus,
    ILogger<OutboxDispatcherHostedService> logger
) : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromSeconds(1);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox dispatcher started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var batch = await store.PeekBatchAsync(BatchSize, stoppingToken);
                foreach (var msg in batch)
                {
                    try
                    {
                        var body = Encoding.UTF8.GetBytes(msg.Payload);
                        await bus.PublishAsync(msg.Topic, body, headers: new Dictionary<string,string> {
                            ["x-msg-id"] = msg.Id.ToString(),
                            ["x-msg-type"] = msg.Type,
                            ["x-occurred-utc"] = msg.OccurredUtc.ToString("O"),
                        }, stoppingToken);

                        await store.MarkDispatchedAsync(msg.Id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Outbox publish failed for {MessageId} (type {Type})", msg.Id, msg.Type);
                        await store.IncrementAttemptAsync(msg.Id, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatcher loop error");
            }

            try { await Task.Delay(Period, stoppingToken); } catch { /* cancelled */ }
        }
        logger.LogInformation("Outbox dispatcher stopped.");
    }
}
