using MediatR;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Monster.Application.Abstractions;

public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : class, ICommand
{
    protected readonly IUnitOfWork Uow;
    protected CommandHandler(IUnitOfWork uow) => Uow = uow;

    protected static Result Ok() => Result.Success();
    protected static Result Fail(string error) => Result.Failure(error);

    public abstract Task<Result> Handle(TCommand request, CancellationToken ct);
}

public abstract class CommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : class, ICommand<TResponse>
{
    protected readonly IUnitOfWork Uow;
    protected CommandHandler(IUnitOfWork uow) => Uow = uow;

    protected static Result<TResponse> Ok(TResponse value) => Result<TResponse>.Success(value);
    protected static Result<TResponse> Fail(string error) => Result<TResponse>.Failure(error);

    public abstract Task<Result<TResponse>> Handle(TCommand request, CancellationToken ct);
}
