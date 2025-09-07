using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Application.Abstractions;
using Identity.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _opt = options.Value;

    public TokenResult IssueToken(string userId, string userName, IEnumerable<string>? roles = null, IEnumerable<Claim>? extraClaims = null)
    {
        var now = DateTime.UtcNow;
        var jti = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        if (roles != null)
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        if (extraClaims != null)
            claims.AddRange(extraClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = now.AddMinutes(_opt.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenResult(jwt, expires, jti);
    }
}
