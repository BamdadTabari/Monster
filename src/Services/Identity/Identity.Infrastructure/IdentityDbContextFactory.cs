using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        // Use env var if provided; fallback to a safe local default
        var cs = Environment.GetEnvironmentVariable("MONSTER_IDENTITY_CS")
                 ?? "Host=localhost;Port=5432;Database=monster_identity;Username=monster;Password=monster;Include Error Detail=true";

        var opts = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(cs, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
            .Options;

        return new IdentityDbContext(opts);
    }
}
