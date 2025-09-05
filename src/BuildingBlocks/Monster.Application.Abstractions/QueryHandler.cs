using MediatR;
using Monster.BuildingBlocks;

namespace Monster.Application.Abstractions;

public abstract class QueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : class, IQuery<TResponse>
{
    protected static Result<TResponse> Ok(TResponse value) => Result<TResponse>.Success(value);
    protected static Result<TResponse> Fail(string error) => Result<TResponse>.Failure(error);

    public abstract Task<Result<TResponse>> Handle(TQuery request, CancellationToken ct);
}
