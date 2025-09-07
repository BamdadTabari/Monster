using MediatR;
using Monster.BuildingBlocks;

namespace Identity.Application.Auth;

public sealed record LoginCommand(string UserNameOrEmail, string Password, string? Ip, string? UserAgent)
    : IRequest<Result<TokenPairDto>>;
