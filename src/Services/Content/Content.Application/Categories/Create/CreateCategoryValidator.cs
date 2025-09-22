namespace Content.Application.Categories.Create;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
         RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");
        RuleFor(x => x.Description).MaximumLength(1024).When(x => x.Description is not null);
    }
}
