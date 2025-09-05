using Content.Domain.Entities;
using Monster.Application.Abstractions;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Content.Application.Categories.Rename;

public sealed class RenameCategoryHandler : CommandHandler<RenameCategoryCommand>
{
    private readonly IRepository<Category> _repo;
    private readonly IUnitOfWork _uow;

    public RenameCategoryHandler(IRepository<Category> repo, IUnitOfWork uow) : base(uow)
    {
        _repo = repo; _uow = uow;
    }

    public override async Task<Result> Handle(RenameCategoryCommand request, CancellationToken ct)
    {
        var entity = await _repo.FirstOrDefaultAsync(c => c.Id == request.Id, asNoTracking: false, ct: ct);
        if (entity is null) return Result.Failure("Category not found.");

        var newName = request.Name.Trim();
        var exists = await _repo.ExistsAsync(c => c.Id != request.Id && c.Name == newName, ct);
        if (exists) return Result.Failure("Category with same name exists.");

        entity.Rename(newName);
        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
