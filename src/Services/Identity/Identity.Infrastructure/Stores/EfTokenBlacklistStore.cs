using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Monster.Persistence.Abstractions;

namespace Identity.Infrastructure.Stores;

public sealed class EfTokenBlacklistStore(
    IRepository<BlacklistedAccessToken> tokens,
    IUnitOfWork uow
) : ITokenBlacklistStore
{
    public async Task BanAsync(string jti, DateTime expiresUtc, CancellationToken ct)
    {
        var exists = await tokens.ExistsAsync(x => x.Jti == jti, ct);
        if (!exists)
        {
            await tokens.AddAsync(new BlacklistedAccessToken(jti, expiresUtc), ct);
            await uow.SaveChangesAsync(ct);
        }
    }

    public Task<bool> IsBannedAsync(string jti, CancellationToken ct)
        => tokens.ExistsAsync(x => x.Jti == jti && x.ExpiresUtc > DateTime.UtcNow, ct);
}
