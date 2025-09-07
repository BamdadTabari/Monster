namespace Identity.Application.Abstractions;

public interface ITokenBlacklistStore
{
    Task BanAsync(string jti, DateTime expiresUtc, CancellationToken ct);
    Task<bool> IsBannedAsync(string jti, CancellationToken ct);
}
