using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using MediatR;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Identity.Application.Auth;

public sealed record RegisterUserCommand(string UserName, string Email, string Password) : IRequest<Result<Guid>>;

public sealed class RegisterUserHandler(
    IRepository<User> users,
    IPasswordHasher hasher,
    IUnitOfWork uow
) : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand r, CancellationToken ct)
    {
        var exists = await users.ExistsAsync(u => u.UserName == r.UserName || u.Email == r.Email.ToLower(), ct);
        if (exists) return Result<Guid>.Failure("User already exists.");

        var entity = new User(r.UserName, r.Email, hasher.Hash(r.Password));
        await users.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(entity.Id);
    }
}
