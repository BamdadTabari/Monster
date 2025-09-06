using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Messaging;

namespace Monster.BuildingBlocks.Outbox;

public sealed class OutboxDispatcherHostedService(
    IOutboxStore store,
    IMessageBusPublisher bus,
    IDateTimeProvider clock,
    ILogger<OutboxDispatcherHostedService> logger
) : BackgroundService
{
    private static readonly TimeSpan LoopPeriod = TimeSpan.FromMilliseconds(300);
    private const int BatchSize = 50;
    private const int PoisonAfterAttempts = 10; // tune if you want

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox dispatcher started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = clock.UtcNow();
                var batch = await store.PeekBatchAsync(BatchSize, now, stoppingToken);

                foreach (var msg in batch)
                {
                    try
                    {
                        await bus.PublishAsync(
                            msg.Topic,
                            Encoding.UTF8.GetBytes(msg.Payload),
                            headers: new Dictionary<string, string> {
                                ["x-msg-id"] = msg.Id.ToString(),
                                ["x-msg-type"] = msg.Type,
                                ["x-occurred-utc"] = msg.OccurredUtc.ToString("O"),
                                ["x-attempt"] = msg.Attempt.ToString()
                            },
                            ct: stoppingToken);

                        await store.MarkDispatchedAsync(msg.Id, clock.UtcNow(), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        var attempt = msg.Attempt + 1;
                        var backoff = OutboxBackoff.ForAttempt(msg.Attempt);
                        var next = clock.UtcNow().Add(backoff);

                        logger.LogWarning(ex, "Outbox publish failed (id={Id}, type={Type}, attempt={Attempt}) → retry at {Next}",
                            msg.Id, msg.Type, attempt, next);

                        await store.IncrementAttemptAsync(msg.Id, attempt, next, stoppingToken);

                        if (attempt >= PoisonAfterAttempts)
                        {
                            logger.LogError("Outbox message considered POISON (id={Id}, type={Type}, attempts={Attempt})", msg.Id, msg.Type, attempt);
                            // Keep it in store with long backoff; or mark dispatched to dead-letter. For now, just long backoff:
                            await store.IncrementAttemptAsync(msg.Id, attempt, clock.UtcNow().Add(TimeSpan.FromHours(1)), stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatcher loop error");
            }

            try { await Task.Delay(LoopPeriod, stoppingToken); } catch { /* cancelled */ }
        }
        logger.LogInformation("Outbox dispatcher stopped.");
    }
}
