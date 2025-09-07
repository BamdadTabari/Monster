namespace Identity.Domain.Entities;

using Monster.BuildingBlocks.Domain;

public sealed class RefreshToken : AuditableEntity
{
    public string Token { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public DateTime ExpiresUtc { get; private set; }
    public DateTime? RevokedUtc { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? CreatedIp { get; private set; }
    public string? UserAgent { get; private set; }

    private RefreshToken() { }

    public RefreshToken(string token, Guid userId, DateTime nowUtc, DateTime expiresUtc, string? ip, string? ua)
    {
        Token = token;
        UserId = userId;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        ExpiresUtc = expiresUtc;
        CreatedIp = ip;
        UserAgent = ua;
    }

    public bool IsActive(DateTime nowUtc) => RevokedUtc is null && ExpiresUtc > nowUtc;
    public void Revoke(DateTime nowUtc, string? replacedBy = null) { RevokedUtc = nowUtc; ReplacedByToken = replacedBy; Touch(); }
}
