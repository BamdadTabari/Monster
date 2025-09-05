using Content.Domain.Entities;
using Monster.Application.Abstractions;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Content.Application.Categories.GetById;

public sealed class GetCategoryByIdHandler : QueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IRepository<Category> _repo;

    public GetCategoryByIdHandler(IRepository<Category> repo) => _repo = repo;

    public override async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        var entity = await _repo.FirstOrDefaultAsync(c => c.Id == request.Id, asNoTracking: true, ct: ct);
        if (entity is null) return Result<CategoryDto>.Failure("Category not found.");

        var dto = Mapper.Map<Category, CategoryDto>(entity);
        return Result<CategoryDto>.Success(dto);
    }
}
