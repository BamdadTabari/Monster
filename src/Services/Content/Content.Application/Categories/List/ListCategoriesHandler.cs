using System.Linq.Expressions;
using Content.Domain.Entities;
using Monster.Application.Abstractions;
using Monster.BuildingBlocks;
using Monster.Persistence.Abstractions;

namespace Content.Application.Categories.List;

public sealed class ListCategoriesHandler : QueryHandler<ListCategoriesQuery, PageResponse<CategoryDto>>
{
    private readonly IRepository<Category> _repo;

    public ListCategoriesHandler(IRepository<Category> repo) => _repo = repo;

    public override async Task<Result<PageResponse<CategoryDto>>> Handle(ListCategoriesQuery request, CancellationToken ct)
    {
        var page = new PageRequest(
            PageNumber: Math.Max(1, request.PageNumber),
            PageSize: Math.Clamp(request.PageSize, 1, 100),
            SortBy: nameof(Category.CreatedAtUtc),
            Desc: true);

        Expression<Func<Category, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var needle = request.Search.Trim().ToLower();
            predicate = c => c.Name.ToLower().Contains(needle);
        }

        var entities = await _repo.PageAsync(page, predicate, asNoTracking: true, ct: ct);
        var mapped = entities.Items.Select(Mapper.Map<Category, CategoryDto>).ToList();
        var dto = new PageResponse<CategoryDto>(mapped, entities.Total, entities.PageNumber, entities.PageSize);
        return Result<PageResponse<CategoryDto>>.Success(dto);
    }
}
