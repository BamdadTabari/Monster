using Microsoft.AspNetCore.Builder;

namespace Monster.BuildingBlocks;

public static class ProblemDetailsExtensions
{
    public static IApplicationBuilder UseGlobalProblemDetails(this IApplicationBuilder app)
        => app.UseMiddleware<ProblemDetailsMiddleware>();
}
