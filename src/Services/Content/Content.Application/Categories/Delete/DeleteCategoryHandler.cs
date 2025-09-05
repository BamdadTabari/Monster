using Content.Domain.Entities;
using Monster.Application.Abstractions;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Content.Application.Categories.Delete;

public sealed class DeleteCategoryHandler : CommandHandler<DeleteCategoryCommand>
{
    private readonly IRepository<Category> _repo;
    private readonly IUnitOfWork _uow;

    public DeleteCategoryHandler(IRepository<Category> repo, IUnitOfWork uow) : base(uow)
    {
        _repo = repo; _uow = uow;
    }

    public override async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var entity = await _repo.FirstOrDefaultAsync(c => c.Id == request.Id, asNoTracking: false, ct: ct);
        if (entity is null) return Result.Failure("Category not found.");

        _repo.Remove(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
