using MediatR;
using Monster.Persistence.Abstractions;

namespace Monster.Application.Abstractions;

/// <summary>Begins a transaction for commands; queries bypass.</summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _uow;

    public TransactionBehavior(IUnitOfWork uow) => _uow = uow;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var isCommand = typeof(TRequest).GetInterfaces().Any(i =>
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)) ||
             i == typeof(ICommand));

        if (!isCommand) return await next();

        await using var tx = await _uow.BeginTransactionAsync(ct);
        var response = await next();
        await tx.CommitAsync(ct);
        return response;
    }
}
