using Identity.Application.Abstractions;
using Identity.Application.Options;
using MediatR;
using Microsoft.Extensions.Options;
using Monster.BuildingBlocks;

namespace Identity.Application.Auth;
public sealed class RefreshHandler(
    IRefreshTokenStore refresh,
    IJwtTokenService jwt,
    IOptions<JwtOptions> jwtOpts
) : IRequestHandler<RefreshTokenCommand, Result<TokenPairDto>>
{
    public async Task<Result<TokenPairDto>> Handle(RefreshTokenCommand r, CancellationToken ct)
    {
        var found = await refresh.GetAsync(r.RefreshToken, ct);
        if (found is null || !found.IsActive(DateTime.UtcNow))
            return Result<TokenPairDto>.Failure("Invalid or expired refresh token.");

        var access = jwt.IssueToken(found.UserId.ToString(), userName: found.UserId.ToString());
        var newRt = await refresh.IssueAsync(found.UserId, DateTime.UtcNow, TimeSpan.FromDays(jwtOpts.Value.RefreshTokenDays), r.Ip, r.UserAgent, ct);
        await refresh.ReplaceAsync(found.Token, newRt, ct);

        return Result<TokenPairDto>.Success(new TokenPairDto(access.AccessToken, access.ExpiresUtc, newRt.Token));
    }
}