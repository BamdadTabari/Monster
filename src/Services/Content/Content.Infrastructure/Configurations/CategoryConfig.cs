using Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Content.Infrastructure.Configurations;

internal sealed class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("categories");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(140);
        b.Property(x => x.Description).HasMaxLength(1024);
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc).IsRequired();

        b.HasIndex(x => x.Name).IsUnique();
        b.HasIndex(x => x.Slug).IsUnique();
    }
}
