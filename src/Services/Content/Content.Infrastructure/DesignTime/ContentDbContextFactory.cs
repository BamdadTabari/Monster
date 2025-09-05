using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Content.Infrastructure.DesignTime;

public sealed class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("CONTENT_CONNECTION")
                 ?? "Host=localhost;Port=5432;Database=monster_content;Username=postgres;Password=postgres";
        var opts = new DbContextOptionsBuilder<ContentDbContext>().UseNpgsql(cs).Options;
        return new ContentDbContext(opts);
    }
}
