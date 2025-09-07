using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Http;
using Identity.Application.Auth; // 👈 add this for ToActionResult()

namespace Identity.Api;

[ApiController]
[Route("auth")]
[Produces("application/json")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResponseDto<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand cmd, CancellationToken ct)
    {
        var res = await mediator.Send(cmd, ct); // Result<Guid>
        return res.ToActionResult("registered", StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResponseDto<TokenPairDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.ToString();

        var res = await mediator.Send(cmd with { Ip = ip, UserAgent = ua }, ct);

        // If you ALWAYS want 401 on login failure, keep it explicit:
        if (!res.IsSuccess)
            return new ObjectResult(new ResponseDto<string>(HttpStatusCode.Unauthorized, res.Error ?? "unauthorized"))
                   { StatusCode = StatusCodes.Status401Unauthorized };

        return res.ToActionResult("ok");
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResponseDto<TokenPairDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand cmd, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.ToString();

        var res = await mediator.Send(cmd with { Ip = ip, UserAgent = ua }, ct);

        if (!res.IsSuccess)
            return new ObjectResult(new ResponseDto<string>(HttpStatusCode.Unauthorized, res.Error ?? "unauthorized"))
                   { StatusCode = StatusCodes.Status401Unauthorized };

        return res.ToActionResult("ok");
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        // sub: prefer Jwt 'sub', fall back to ClaimTypes.NameIdentifier
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? string.Empty;

        if (!Guid.TryParse(sub, out var userId))
            return new ObjectResult(new ResponseDto<string>(HttpStatusCode.BadRequest, "invalid subject"))
                   { StatusCode = StatusCodes.Status400BadRequest };

        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? string.Empty;

        // exp: parse safely
        var expUnix = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        DateTime expUtc = DateTime.UtcNow.AddMinutes(60);
        if (long.TryParse(expUnix, out var expSeconds))
            expUtc = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;

        var res = await mediator.Send(new LogoutCommand(userId, jti, expUtc), ct);

        if (!res.IsSuccess)
            return res.ToActionResult(); // defaults to 400 with ProblemDetails

        return NoContent();
    }
}
