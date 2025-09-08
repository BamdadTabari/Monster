using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Outbox;

namespace Content.Infrastructure;

/// <summary>
/// EF Core DbContext for Content service.
/// At this stage we only include OutboxMessages; domain tables come later.
/// </summary>
public sealed class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("content");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);

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
