namespace Identity.Application.Options;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "monster";
    public string Audience { get; set; } = "monster-api";
    public string SigningKey { get; set; } = "very-long-dev-secret-change-me-123";
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 14;
}
