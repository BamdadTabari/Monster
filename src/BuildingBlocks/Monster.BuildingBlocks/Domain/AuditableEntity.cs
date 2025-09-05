namespace Monster.BuildingBlocks.Domain;

/// <summary>Entity with created/updated audit fields and optional soft-delete.</summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; protected set; } = DateTime.UtcNow;

    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAtUtc { get; protected set; }

    protected void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    public virtual void SoftDelete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        Touch();
    }
}
