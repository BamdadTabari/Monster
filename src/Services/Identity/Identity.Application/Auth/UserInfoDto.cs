namespace Identity.Application;

public sealed record UserInfoDto(Guid UserId, string UserName, IReadOnlyCollection<string> Roles);