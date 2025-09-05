using Monster.Application.Abstractions;

namespace Content.Application.Categories.Delete;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
