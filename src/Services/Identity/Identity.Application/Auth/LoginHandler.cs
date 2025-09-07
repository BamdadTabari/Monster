using Identity.Application.Abstractions;
using Identity.Application.Options;
using Identity.Domain;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Identity.Application.Auth;
public sealed class LoginHandler(
    IRepository<User> users,
    IPasswordHasher hasher,
    IJwtTokenService jwt,
    IRefreshTokenStore refresh,
    IOptions<JwtOptions> jwtOpts
) : IRequestHandler<LoginCommand, Result<TokenPairDto>>
{
    public async Task<Result<TokenPairDto>> Handle(LoginCommand r, CancellationToken ct)
    {
        var user = await users.FirstOrDefaultAsync(
            u => u.UserName == r.UserNameOrEmail || u.Email == r.UserNameOrEmail.ToLower(), ct: ct);

        if (user is null || !user.IsActive || !hasher.Verify(r.Password, user.PasswordHash))
            return Result<TokenPairDto>.Failure("Invalid credentials.");

        var access = jwt.IssueToken(user.Id.ToString(), user.UserName, roles: null);
        var rt = await refresh.IssueAsync(user.Id, DateTime.UtcNow, TimeSpan.FromDays(jwtOpts.Value.RefreshTokenDays), r.Ip, r.UserAgent, ct);

        return Result<TokenPairDto>.Success(new TokenPairDto(access.AccessToken, access.ExpiresUtc, rt.Token));
    }
}