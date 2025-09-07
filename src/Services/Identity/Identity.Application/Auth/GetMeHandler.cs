using MediatR;
using Monster.BuildingBlocks;

namespace Identity.Application;
public sealed class GetMeHandler : IRequestHandler<GetMeQuery, Result<UserInfoDto>>
{
    public Task<Result<UserInfoDto>> Handle(GetMeQuery q, CancellationToken ct)
        => Task.FromResult(Result<UserInfoDto>.Success(new UserInfoDto(q.UserId, q.UserName, q.Roles)));
}