using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Monster.BuildingBlocks;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    IEnumerable<string> Roles { get; }
}

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _ctx;
    public HttpCurrentUser(IHttpContextAccessor ctx) => _ctx = ctx;
    public bool IsAuthenticated => _ctx.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    public string? UserId => _ctx.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public IEnumerable<string> Roles => _ctx.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(r => r.Value) ?? Enumerable.Empty<string>();
}
