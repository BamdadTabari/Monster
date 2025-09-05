using Content.Application.Categories.GetById;
using Content.Domain.Entities;
using Content.Application.Tests.Fakes;
using FluentAssertions;

namespace Content.Application.Tests;

public class GetCategoryByIdHandlerTests
{
    [Fact]
    public async Task Returns_dto_when_found()
    {
        var repo = new FakeRepository<Category>();
        var id = Guid.NewGuid();
        repo.Seed(new Category(id, "Tech", "desc"));

        var h = new GetCategoryByIdHandler(repo);

        var res = await h.Handle(new GetCategoryByIdQuery(id), default);

        res.IsSuccess.Should().BeTrue();
        res.Value!.Id.Should().Be(id);
        res.Value!.Name.Should().Be("Tech");
    }

    [Fact]
    public async Task Fails_when_not_found()
    {
        var repo = new FakeRepository<Category>();
        var h = new GetCategoryByIdHandler(repo);

        var res = await h.Handle(new GetCategoryByIdQuery(Guid.NewGuid()), default);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().NotBeNullOrWhiteSpace();
    }
}
