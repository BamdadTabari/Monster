using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Monster.BuildingBlocks;
using Xunit;

namespace Monster.BuildingBlocks.Tests;

public class ValidationBehaviorTests
{
    private record MakeThing(string Name) : IRequest<string>;

    private class MakeThingValidator : AbstractValidator<MakeThing>
    {
        public MakeThingValidator() => RuleFor(x => x.Name).NotEmpty();
    }

    [Fact]
    public async Task Throws_on_validation_failure()
    {
        var behavior = new ValidationBehavior<MakeThing, string>(new[] { new MakeThingValidator() });
        var act = async () => await behavior.Handle(new MakeThing(""), (ct) => Task.FromResult("ok"), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Passes_through_on_success()
    {
        var behavior = new ValidationBehavior<MakeThing, string>(new[] { new MakeThingValidator() });
        var result = await behavior.Handle(new MakeThing("ok"), (ct) => Task.FromResult("ok"), CancellationToken.None);
        result.Should().Be("ok");
    }
}
