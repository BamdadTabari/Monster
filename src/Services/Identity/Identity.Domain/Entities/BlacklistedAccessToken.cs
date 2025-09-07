namespace Identity.Domain.Entities;

using Monster.BuildingBlocks.Domain;

public sealed class BlacklistedAccessToken : AuditableEntity
{
    public string Jti { get; private set; } = default!;
    public DateTime ExpiresUtc { get; private set; }
    public string? Reason { get; private set; }

    private BlacklistedAccessToken() { }
    public BlacklistedAccessToken(string jti, DateTime expiresUtc, string? reason = null)
    { Jti = jti; ExpiresUtc = expiresUtc; Reason = reason; }
}
