using Monster.Application.Abstractions;

namespace Content.Application.Categories.Rename;

public sealed record RenameCategoryCommand(Guid Id, string Name) : ICommand;
