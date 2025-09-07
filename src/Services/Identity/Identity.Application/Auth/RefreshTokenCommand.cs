using MediatR;
using Monster.BuildingBlocks;

namespace Identity.Application.Auth;

public sealed record RefreshTokenCommand(string RefreshToken, string? Ip, string? UserAgent)
    : IRequest<Result<TokenPairDto>>;