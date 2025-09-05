using MediatR;
using Monster.BuildingBlocks;

namespace Monster.Application.Abstractions;

public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
