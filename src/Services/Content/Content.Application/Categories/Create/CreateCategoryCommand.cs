using Monster.Application.Abstractions;

namespace Content.Application.Categories.Create;

public sealed record CreateCategoryCommand(string Name, string? Description) : ICommand<Guid>;
