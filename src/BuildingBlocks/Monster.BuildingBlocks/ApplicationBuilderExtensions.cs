using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Monster.BuildingBlocks;

public static class ApplicationBuilderExtensions
{
    /// <summary>Standard web pipeline pieces: request logging, ProblemDetails, correlation id.</summary>
    public static IApplicationBuilder UseMonsterWebPipeline(this IApplicationBuilder app)
        => app
            .UseSerilogRequestLogging()
            .UseProblemDetails()
            .UseCorrelationId();
}
