using Content.Application.Categories.Rename;
using Content.Domain.Entities;
using Content.Application.Tests.Fakes;
using FluentAssertions;

namespace Content.Application.Tests;

public class RenameCategoryHandlerTests
{
    [Fact]
    public async Task Renames_when_exists_and_unique()
    {
        var repo = new FakeRepository<Category>();
        var id = Guid.NewGuid();
        repo.Seed(new Category(id, "Old", null));

        var h = new RenameCategoryHandler(repo, new FakeUnitOfWork());

        var res = await h.Handle(new RenameCategoryCommand(id, "New"), default);

        res.IsSuccess.Should().BeTrue();
        // simple state check: entity in fake repo updated
        (await repo.FirstOrDefaultAsync(c => c.Id == id))!.Name.Should().Be("New");
    }

    [Fact]
    public async Task Fails_when_target_missing()
    {
        var repo = new FakeRepository<Category>();
        var h = new RenameCategoryHandler(repo, new FakeUnitOfWork());

        var res = await h.Handle(new RenameCategoryCommand(Guid.NewGuid(), "Any"), default);

        res.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Fails_when_new_name_already_taken()
    {
        var repo = new FakeRepository<Category>();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        repo.Seed(
            new Category(id1, "Tech", null),
            new Category(id2, "Sports", null)
        );

        var h = new RenameCategoryHandler(repo, new FakeUnitOfWork());

        var res = await h.Handle(new RenameCategoryCommand(id1, "Sports"), default);

        res.IsSuccess.Should().BeFalse();
    }
}
