using FluentValidation;

namespace Content.Application.Categories.Rename;

public sealed class RenameCategoryValidator : AbstractValidator<RenameCategoryCommand>
{
    public RenameCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
