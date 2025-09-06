using Content.Application.Categories.Delete;
using Content.Domain.Entities;
using FluentAssertions;
using Monster.Testing.Fakes;

namespace Content.Application.Tests;

public class DeleteCategoryHandlerTests
{
    [Fact]
    public async Task Deletes_when_found()
    {
        var repo = new FakeRepository<Category>();
        var id = Guid.NewGuid();
        var entity = new Category(id, "Tech", null);
        repo.Seed(entity);

        var h = new DeleteCategoryHandler(repo, new FakeUnitOfWork());

        var res = await h.Handle(new DeleteCategoryCommand(id), default);

        res.IsSuccess.Should().BeTrue();
        (await repo.FirstOrDefaultAsync(c => c.Id == id)).Should().BeNull();
    }

    [Fact]
    public async Task Fails_when_missing()
    {
        var repo = new FakeRepository<Category>();
        var h = new DeleteCategoryHandler(repo, new FakeUnitOfWork());

        var res = await h.Handle(new DeleteCategoryCommand(Guid.NewGuid()), default);

        res.IsSuccess.Should().BeFalse();
    }
}
