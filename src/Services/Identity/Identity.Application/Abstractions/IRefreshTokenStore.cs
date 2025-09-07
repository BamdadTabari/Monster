using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

public interface IRefreshTokenStore
{
    Task<RefreshToken> IssueAsync(Guid userId, DateTime nowUtc, TimeSpan ttl, string? ip, string? ua, CancellationToken ct);
    Task<RefreshToken?> GetAsync(string token, CancellationToken ct);
    Task ReplaceAsync(string oldToken, RefreshToken newToken, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, DateTime nowUtc, CancellationToken ct);
}
