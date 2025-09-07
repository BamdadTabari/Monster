using System.Security.Cryptography;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Monster.Persistence.Abstractions;

namespace Identity.Infrastructure.Stores;

public sealed class EfRefreshTokenStore(
    IRepository<RefreshToken> tokens,
    IUnitOfWork uow
) : IRefreshTokenStore
{
    public async Task<RefreshToken> IssueAsync(Guid userId, DateTime nowUtc, TimeSpan ttl, string? ip, string? ua, CancellationToken ct)
    {
        var token = Base64Url(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken(token, userId, nowUtc, nowUtc.Add(ttl), ip, ua);
        await tokens.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity;
    }

    public Task<RefreshToken?> GetAsync(string token, CancellationToken ct)
        => tokens.FirstOrDefaultAsync(x => x.Token == token, ct: ct);

    public async Task ReplaceAsync(string oldToken, RefreshToken newToken, CancellationToken ct)
    {
        var current = await tokens.FirstOrDefaultAsync(x => x.Token == oldToken, ct: ct);
        if (current is not null) current.Revoke(DateTime.UtcNow, newToken.Token);
        await tokens.AddAsync(newToken, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, DateTime nowUtc, CancellationToken ct)
    {
        var list = await tokens.ListAsync(x => x.UserId == userId && x.RevokedUtc == null, ct: ct);
        foreach (var rt in list) rt.Revoke(nowUtc);
        await uow.SaveChangesAsync(ct);
    }

    private static string Base64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
