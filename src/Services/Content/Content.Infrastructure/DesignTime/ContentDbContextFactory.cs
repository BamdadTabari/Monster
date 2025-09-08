using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Content.Infrastructure.DesignTime;

public sealed class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("CONTENT_CONNECTION")
                 ?? "Host=localhost;Port=5432;Database=monster_content;Username=monster;Password=monster";
        var opts = new DbContextOptionsBuilder<ContentDbContext>().UseNpgsql(cs, npg =>
        {
            // history table line if you use schemas:
            npg.MigrationsHistoryTable("__EFMigrationsHistory", "content");
            npg.EnableRetryOnFailure( // defaults are fine; tweak as you wish
                maxRetryCount: 6,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }).Options;
        return new ContentDbContext(opts);
    }
}
