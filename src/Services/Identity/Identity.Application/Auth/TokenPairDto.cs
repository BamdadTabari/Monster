namespace Identity.Application.Auth;
public sealed record TokenPairDto(string AccessToken, DateTime AccessExpiresUtc, string RefreshToken);
