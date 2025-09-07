using Identity.Domain;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserName).IsUnique();
        b.HasIndex(x => x.Email).IsUnique();

        b.Property(x => x.UserName).HasMaxLength(64).IsRequired();
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc).IsRequired();
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}
