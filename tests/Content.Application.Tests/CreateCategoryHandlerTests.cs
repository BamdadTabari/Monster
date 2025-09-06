using Content.Application.Categories.Create;
using Content.Domain.Entities;
using FluentAssertions;
using Monster.Testing.Fakes;

namespace Content.Application.Tests;

public class CreateCategoryHandlerTests
{
    [Fact]
    public async Task Creates_category_with_id_from_IIdGenerator()
    {
        var repo = new FakeRepository<Category>();
        var uow = new FakeUnitOfWork();
        var expectedId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var ids = new FakeIdGenerator(expectedId);
        var events = new FakeIntegrationEventPublisher();

        var handler = new CreateCategoryHandler(repo, uow, ids, events);

        var result = await handler.Handle(new CreateCategoryCommand("Tech", "desc"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);
        events.Published.Should().HaveCount(1); // optional: prove event was emitted
    }

    [Fact]
    public async Task Fails_when_name_already_exists()
    {
        var repo = new FakeRepository<Category>();
        var uow = new FakeUnitOfWork();
        var ids = new FakeIdGenerator(Guid.NewGuid());
        var events = new FakeIntegrationEventPublisher();

        repo.Seed(new Category(Guid.NewGuid(), "Tech", "d1"));

        var handler = new CreateCategoryHandler(repo, uow, ids, events);

        var result = await handler.Handle(new CreateCategoryCommand("Tech", null), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
        events.Published.Should().BeEmpty(); // optional
    }
}
