using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monster.BuildingBlocks.Errors;

namespace Monster.BuildingBlocks;

public static class ProblemDetailsSetup
{
    public static IServiceCollection AddStandardProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.IncludeExceptionDetails = (ctx, _) =>
                ctx.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment();

            options.Map<AppException>((ctx, ex) =>
            {
                var pd = new ProblemDetails
                {
                    Title = ex.Error.Code,
                    Detail = ex.Error.Message,
                    Status = (int)ex.StatusCode
                };
                if (ex.Error.Details is not null)
                    pd.Extensions["errors"] = ex.Error.Details;
                return pd;
            });

            options.MapToStatusCode<UnauthorizedAccessException>(StatusCodes.Status401Unauthorized);
            options.MapToStatusCode<KeyNotFoundException>(StatusCodes.Status404NotFound);
            options.MapToStatusCode<ArgumentException>(StatusCodes.Status400BadRequest);
            options.MapToStatusCode<FluentValidation.ValidationException>(StatusCodes.Status400BadRequest);
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        });
        return services;
    }
}
