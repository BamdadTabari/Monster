using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monster.BuildingBlocks;

namespace Identity.Api;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand cmd, CancellationToken ct)
    {
        var res = await mediator.Send(cmd, ct);
        return res.IsSuccess
            ? Ok(new ResponseDto<Guid>(System.Net.HttpStatusCode.OK, "registered", res.Value))
            : BadRequest(new ResponseDto<string>(System.Net.HttpStatusCode.BadRequest, res.Error ?? "error"));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd, CancellationToken ct)
    {
        // enrich command with ip/ua from server
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        var res = await mediator.Send(cmd with { Ip = ip, UserAgent = ua }, ct);

        return res.IsSuccess
            ? Ok(new ResponseDto<TokenPairDto>(System.Net.HttpStatusCode.OK, "ok", res.Value))
            : Unauthorized(new ResponseDto<string>(System.Net.HttpStatusCode.Unauthorized, res.Error ?? "unauthorized"));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand cmd, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        var res = await mediator.Send(cmd with { Ip = ip, UserAgent = ua }, ct);

        return res.IsSuccess
            ? Ok(new ResponseDto<TokenPairDto>(System.Net.HttpStatusCode.OK, "ok", res.Value))
            : Unauthorized(new ResponseDto<string>(System.Net.HttpStatusCode.Unauthorized, res.Error ?? "unauthorized"));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "";
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? "";
        var expUnix = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        var exp = string.IsNullOrEmpty(expUnix) ? DateTime.UtcNow.AddMinutes(60) : DateTimeOffset.FromUnixTimeSeconds(long.Parse(expUnix)).UtcDateTime;

        if (!Guid.TryParse(sub, out var userId))
            return BadRequest(new ResponseDto<string>(System.Net.HttpStatusCode.BadRequest, "invalid sub"));

        var res = await mediator.Send(new LogoutCommand(userId, jti, exp), ct);
        return res.IsSuccess ? NoContent() : BadRequest(new ResponseDto<string>(System.Net.HttpStatusCode.BadRequest, res.Error ?? "error"));
    }
}
