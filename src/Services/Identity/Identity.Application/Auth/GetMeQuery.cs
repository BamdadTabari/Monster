using MediatR;
using Monster.BuildingBlocks;

namespace Identity.Application;

public sealed record GetMeQuery(Guid UserId, string UserName, IReadOnlyCollection<string> Roles)
    : IRequest<Result<UserInfoDto>>;