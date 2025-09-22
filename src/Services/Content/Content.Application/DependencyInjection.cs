using Microsoft.Extensions.DependencyInjection;
using Monster.Application.Abstractions;

namespace Content.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddContentApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AssemblyMarker>());
        services.AddValidatorsFromAssemblyContaining<AssemblyMarker>(); 
        services.AddMonsterApplicationCore();                                       
        return services;
    }
}
