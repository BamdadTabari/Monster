namespace Identity.Domain.Entities;

using Monster.BuildingBlocks.Domain;

public sealed class User : AuditableEntity
{
    public string UserName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private User() { }

    public User(string userName, string email, string passwordHash)
    {
        UserName = userName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
    }

    public void SetPasswordHash(string hash) { PasswordHash = hash; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
}
