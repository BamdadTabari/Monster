using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Configurations;

public sealed class BlacklistedAccessTokenConfiguration : IEntityTypeConfiguration<BlacklistedAccessToken>
{
    public void Configure(EntityTypeBuilder<BlacklistedAccessToken> b)
    {
        b.ToTable("blacklisted_access_tokens");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Jti).IsUnique();

        b.Property(x => x.Jti).HasMaxLength(64).IsRequired();
        b.Property(x => x.ExpiresUtc).IsRequired();
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc).IsRequired();
    }
}
