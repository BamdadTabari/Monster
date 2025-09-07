using MediatR;
using Monster.BuildingBlocks;

namespace Identity.Application.Auth;

public sealed record LogoutCommand(Guid UserId, string Jti, DateTime AccessExpiresUtc) : IRequest<Result>;