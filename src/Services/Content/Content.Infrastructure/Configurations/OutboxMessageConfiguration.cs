using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monster.BuildingBlocks.Outbox;

namespace Content.Infrastructure.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outbox");
        b.HasKey(x => x.Id);

        b.Property(x => x.Type).HasMaxLength(128).IsRequired();
        b.Property(x => x.Topic).HasMaxLength(256).IsRequired();
        b.Property(x => x.Payload).IsRequired();

        b.Property(x => x.OccurredUtc).IsRequired();
        b.Property(x => x.DispatchedUtc);
        b.Property(x => x.Attempt).IsRequired();
        b.Property(x => x.NextAttemptUtc).IsRequired();
    }
}
