using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Monster.BuildingBlocks.Behaviors;
using Monster.BuildingBlocks.Messaging;
using Monster.BuildingBlocks.Outbox;

namespace Monster.BuildingBlocks;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers BuildingBlocks services (ProblemDetails, Validation pipeline, HttpContext accessor, time/id providers).</summary>
    public static IServiceCollection AddMonsterBuildingBlocks(this IServiceCollection services)
    {
        services.AddStandardProblemDetails();
        services.AddHttpContextAccessor();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();

        // MediatR Validation pipeline (consumer must add MediatR scanning separately)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>(); // if you have one
        services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();
        services.AddSingleton<IMessageBusPublisher, InMemoryMessageBusPublisher>();
        services.AddSingleton<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
        services.AddHostedService<OutboxDispatcherHostedService>();
        
        return services;
    }
}
