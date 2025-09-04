using Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers EF Core DbContext and infra services for Content.</summary>
    public static IServiceCollection AddContentInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Content") ??
                 "Host=localhost;Port=5432;Database=monster_content;Username=postgres;Password=postgres";

        services.AddDbContext<ContentDbContext>(opt =>
            opt.UseNpgsql(cs));

         // Repositories + Unit of Work (open generics)
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        return services;
    }
}
