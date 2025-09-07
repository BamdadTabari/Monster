using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Outbox;

namespace Identity.Infrastructure;

/// <summary>
/// EF Core DbContext for Identity service.
/// For now, only OutboxMessages; user/role tables come later.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
