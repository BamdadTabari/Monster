using Monster.BuildingBlocks.Outbox;

namespace Content.Application.Categories.Events;

public sealed record CategoryCreated(Guid CategoryId, string Name) : IntegrationEvent;
