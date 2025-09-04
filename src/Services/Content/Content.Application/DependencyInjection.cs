using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Application;

public static class DependencyInjection
{
    /// <summary>Registers MediatR handlers & validators located in this assembly.</summary>
    public static IServiceCollection AddContentApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IApplicationAssemblyMarker>());
        // Validators are discovered via FluentValidation when you add them per-request type.
        return services;
    }
}
