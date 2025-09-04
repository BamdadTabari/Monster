using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Monster.BuildingBlocks;

/// <summary>Adds/propagates X-Correlation-ID and enriches Serilog scope.</summary>
public sealed class CorrelationIdMiddleware
{
    public const string Header = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        if (!ctx.Request.Headers.TryGetValue(Header, out var cid))
            cid = Guid.NewGuid().ToString("N");

        ctx.Response.Headers[Header] = cid!;
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", cid.ToString()))
            await _next(ctx);
    }
}

public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
