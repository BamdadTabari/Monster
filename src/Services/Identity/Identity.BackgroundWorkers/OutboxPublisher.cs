using Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Identity.BackgroundWorkers;

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
        _log.LogInformation("Identity OutboxPublisher started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                // Fetch & publish pending outbox messages here later
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in OutboxPublisher loop.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        _log.LogInformation("Identity OutboxPublisher stopping.");
    }
}
