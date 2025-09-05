using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monster.Persistence.Abstractions;
using Identity.Infrastructure.Persistence;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Identity") ??
                 "Host=localhost;Port=5432;Database=monster_identity;Username=postgres;Password=postgres";

        services.AddDbContext<IdentityDbContext>(opt => opt.UseNpgsql(cs));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
