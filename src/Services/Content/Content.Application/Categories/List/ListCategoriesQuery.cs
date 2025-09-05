using Monster.Application.Abstractions;
using Monster.BuildingBlocks;

namespace Content.Application.Categories.List;

public sealed record ListCategoriesQuery(int PageNumber = 1, int PageSize = 20, string? Search = null)
    : IQuery<PageResponse<CategoryDto>>;
