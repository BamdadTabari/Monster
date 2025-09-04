using Content.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Content.BackgroundWorkers;

/// <summary>
/// Outbox publisher skeleton. When you add domain events and a message bus,
/// implement fetch N unprocessed → publish → mark processed.
/// </summary>
public sealed class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<OutboxPublisher> _log;

    public OutboxPublisher(IServiceProvider sp, ILogger<OutboxPublisher> log)
    {
        _sp = sp; _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("OutboxPublisher started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

                // No-op for now: this is the place to publish pending outbox messages.
                // e.g., var pending = await db.OutboxMessages.Where(x => x.ProcessedOnUtc == null).Take(50).ToListAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in OutboxPublisher loop.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        _log.LogInformation("OutboxPublisher stopping.");
    }
}
