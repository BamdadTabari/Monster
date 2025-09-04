using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Monster.BuildingBlocks;

/// <summary>Standard RFC7807 error responses with sensible mappings.</summary>
public static class ProblemDetailsSetup
{
    public static IServiceCollection AddStandardProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.IncludeExceptionDetails = (ctx, _) =>
                ctx.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment();

            options.MapToStatusCode<UnauthorizedAccessException>(StatusCodes.Status401Unauthorized);
            options.MapToStatusCode<KeyNotFoundException>(StatusCodes.Status404NotFound);
            options.MapToStatusCode<ArgumentException>(StatusCodes.Status400BadRequest);
            options.MapToStatusCode<FluentValidation.ValidationException>(StatusCodes.Status400BadRequest);
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        });
        return services;
    }
}