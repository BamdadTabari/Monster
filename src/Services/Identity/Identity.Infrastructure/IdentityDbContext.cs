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

        foreach (var et in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Monster.BuildingBlocks.Domain.AuditableEntity).IsAssignableFrom(et.ClrType))
            {
                // Use non-generic since type is only known at runtime
                modelBuilder.Entity(et.ClrType)
                    .Property<uint>(nameof(Monster.BuildingBlocks.Domain.AuditableEntity.Version))
                    .IsRowVersion();   // Npgsql maps uint rowversion -> PostgreSQL xmin
            }
        }
    }
}
