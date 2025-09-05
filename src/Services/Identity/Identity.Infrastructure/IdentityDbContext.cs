using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;

namespace Identity.Infrastructure;

/// <summary>
/// EF Core DbContext for Identity service.
/// For now, only OutboxMessages; user/role tables come later.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<OutboxMessage>(cfg =>
        {
            cfg.ToTable("outbox_messages");
            cfg.HasKey(x => x.Id);
            cfg.Property(x => x.Type).IsRequired();
            cfg.Property(x => x.Payload).IsRequired();
            cfg.Property(x => x.OccurredOnUtc).IsRequired();
            cfg.Property(x => x.ProcessedOnUtc);
            cfg.Property(x => x.Error);
        });
    }
}
