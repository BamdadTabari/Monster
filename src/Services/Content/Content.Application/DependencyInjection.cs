using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Monster.Application.Abstractions;

namespace Content.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddContentApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IApplicationAssemblyMarker>());
        services.AddValidatorsFromAssemblyContaining<IApplicationAssemblyMarker>();   // ← add this
        services.AddMonsterApplicationCore();                                        // transaction behavior
        return services;
    }
}
