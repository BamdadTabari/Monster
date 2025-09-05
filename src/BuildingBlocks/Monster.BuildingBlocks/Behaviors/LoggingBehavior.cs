using MediatR;
using Microsoft.Extensions.Logging;

namespace Monster.BuildingBlocks.Behaviors;

/// <summary>Logs request start/finish/failure with elapsed time.</summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _log;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> log) => _log = log;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            _log.LogInformation("Handling {Request}", name);
            var resp = await next();
            sw.Stop();
            _log.LogInformation("Handled {Request} in {Elapsed}ms", name, sw.ElapsedMilliseconds);
            return resp;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogError(ex, "Error handling {Request} after {Elapsed}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
