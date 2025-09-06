using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Monster.BuildingBlocks.Messaging;

namespace Content.Api.Tests;

public class EventTests: IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

    public EventTests(WebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Creating_category_emits_CategoryCreated_event()
    {
        InMemoryMessageBusPublisher.Published.Clear();
        var payload = new { name = "Tech", description = "desc" };

        var resp = await _client.PostAsJsonAsync("/api/categories", payload);
        resp.EnsureSuccessStatusCode();

        await Task.Delay(1500); // let OutboxDispatcher flush

        InMemoryMessageBusPublisher.Published.Should()
            .ContainSingle(x => x.Topic == "content.category.created");

        var msg = InMemoryMessageBusPublisher.Published.Single(x => x.Topic == "content.category.created");
        using var json = JsonDocument.Parse(msg.Body);
        json.RootElement.GetProperty("categoryId").GetGuid().Should().NotBe(Guid.Empty);
        json.RootElement.GetProperty("name").GetString().Should().Be("Tech");
    }
}