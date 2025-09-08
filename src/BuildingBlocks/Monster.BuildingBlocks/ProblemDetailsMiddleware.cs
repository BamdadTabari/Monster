using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Monster.BuildingBlocks;

public sealed class ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            var (status, title) = MapException(ex);
            logger.LogError(ex, "Unhandled exception -> {Status}", status);

            var problem = new ProblemDetails
            {
                Status = status,
                Title  = title,
                Detail = ex.Message,
                Type   = $"https://httpstatuses.io/{status}",
                Instance = ctx.TraceIdentifier
            };

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsJsonAsync(problem);
        }
    }

    private static (int status, string title) MapException(Exception ex)
    {
        // Walk inner exceptions looking for Npgsql.PostgresException (by FullName, no hard ref)
        for (var e = ex; e is not null; e = e.InnerException!)
        {
            var fullName = e.GetType().FullName;
            if (fullName == "Npgsql.PostgresException")
            {
                var sqlState = e.GetType().GetProperty("SqlState")?.GetValue(e)?.ToString();
                return sqlState switch
                {
                    "23505" => (StatusCodes.Status409Conflict, "resource.conflict"),        // unique violation
                    "23503" => (StatusCodes.Status409Conflict, "foreign_key.conflict"),    // FK violation
                    _       => (StatusCodes.Status400BadRequest, "db.error")
                };
            }
        }

        // Validation-ish
        if (ex is ArgumentException or InvalidOperationException)
            return (StatusCodes.Status422UnprocessableEntity, "request.validation_failed");

        // Auth-ish
        if (ex is UnauthorizedAccessException)
            return (StatusCodes.Status401Unauthorized, "auth.unauthorized");

        return (StatusCodes.Status500InternalServerError, "server.error");
    }
}