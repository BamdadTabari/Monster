using Identity.Application.Abstractions;
using MediatR;
using Monster.BuildingBlocks;

namespace Identity.Application.Auth;
public sealed class LogoutHandler(ITokenBlacklistStore blacklist, IRefreshTokenStore refresh)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand r, CancellationToken ct)
    {
        await blacklist.BanAsync(r.Jti, r.AccessExpiresUtc, ct);
        await refresh.RevokeAllForUserAsync(r.UserId, DateTime.UtcNow, ct);
        return Result.Success();
    }
}