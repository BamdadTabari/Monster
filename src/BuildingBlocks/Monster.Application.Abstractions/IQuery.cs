using MediatR;
using Monster.BuildingBlocks;

namespace Monster.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
