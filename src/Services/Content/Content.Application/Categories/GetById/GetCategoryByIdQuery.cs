using Monster.Application.Abstractions;

namespace Content.Application.Categories.GetById;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryDto>;
