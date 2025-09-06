using Content.Application.Categories.List;
using Content.Domain.Entities;
using FluentAssertions;
using Monster.Testing.Fakes;

namespace Content.Application.Tests;

public class ListCategoriesHandlerTests
{
    [Fact]
    public async Task Returns_paged_results_and_maps_to_dto()
    {
        var repo = new FakeRepository<Category>();
        var h = new ListCategoriesHandler(repo);

        repo.Seed(
            new Category(Guid.NewGuid(), "Tech", null),
            new Category(Guid.NewGuid(), "Food", null),
            new Category(Guid.NewGuid(), "Travel", null)
        );

        var result = await h.Handle(new ListCategoriesQuery(1, 2, null), default);

        result.IsSuccess.Should().BeTrue();
        var page = result.Value!;
        page.Items.Count.Should().Be(2);
        page.Total.Should().Be(3);
        page.PageNumber.Should().Be(1);
        page.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task Filters_by_search_term()
    {
        var repo = new FakeRepository<Category>();
        var h = new ListCategoriesHandler(repo);

        repo.Seed(
            new Category(Guid.NewGuid(), "Tech", null),
            new Category(Guid.NewGuid(), "Sports", null)
        );

        var result = await h.Handle(new ListCategoriesQuery(1, 10, "spo"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle(i => i.Name == "Sports");
    }
}
