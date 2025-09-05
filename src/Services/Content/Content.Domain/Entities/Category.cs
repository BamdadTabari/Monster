using Monster.BuildingBlocks.Domain;
using Monster.BuildingBlocks.Text;

namespace Content.Domain.Entities;

public sealed class Category : AuditableEntity
{
    private Category() { } // EF

    public Category(Guid id, string name, string? description = null)
    {
        if (id == default) throw new ArgumentException("Id must be provided by the application layer.", nameof(id));
        Id = id;
        Rename(name);
        SetDescription(description);
        // Created/Updated are handled by DbContext auditing
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        var trimmed = name.Trim();
        if (trimmed.Length > 128)
            throw new ArgumentException("Category name is too long (max 128).", nameof(name));

        Name = trimmed;
        Slug = SlugHelper.ToSlug(trimmed);
        Touch();
        // Raise(new CategoryRenamed(Id, Name)); // later
    }

    public void SetDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Touch();
    }
}
