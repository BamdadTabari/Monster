using System.Security.Claims;

namespace Identity.Application.Abstractions;

public sealed record TokenResult(string AccessToken, DateTime ExpiresUtc, string Jti);

public interface IJwtTokenService
{
    TokenResult IssueToken(string userId, string userName, IEnumerable<string>? roles = null, IEnumerable<Claim>? extraClaims = null);
}
