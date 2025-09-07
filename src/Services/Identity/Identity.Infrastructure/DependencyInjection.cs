using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monster.Persistence.Abstractions;
using Identity.Infrastructure.Persistence;
using Identity.Application.Options;
using Identity.Application.Abstractions;
using Identity.Infrastructure.Security;
using Identity.Infrastructure.Stores;
using Monster.BuildingBlocks.Outbox;
using Identity.Infrastructure.Outbox;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("IdentityDb")
                 ?? "Host=localhost;Port=5432;Database=monster_identity;Username=monster;Password=monster";

        services.AddDbContext<IdentityDbContext>(opt => opt.UseNpgsql(cs));

        // repos + uow
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // security
        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PBKDF2PasswordHasher>();
        services.AddScoped<IRefreshTokenStore, EfRefreshTokenStore>();
        services.AddScoped<ITokenBlacklistStore, EfTokenBlacklistStore>();

        // outbox
        services.AddScoped<IOutboxStore, EfOutboxStore>();

        return services;
    }
}
