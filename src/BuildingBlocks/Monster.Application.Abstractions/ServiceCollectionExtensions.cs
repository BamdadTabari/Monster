using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Monster.Application.Abstractions;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers shared application pipeline behaviors (transaction).</summary>
    public static IServiceCollection AddMonsterApplicationCore(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        return services;
    }
}
