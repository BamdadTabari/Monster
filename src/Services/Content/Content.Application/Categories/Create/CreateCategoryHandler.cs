using Content.Domain.Entities;
using Monster.Persistence.Abstractions;
using Monster.BuildingBlocks;
using Monster.Application.Abstractions;
using Monster.BuildingBlocks.Outbox;
using Content.Application.Categories.Events; // IIdGenerator

namespace Content.Application.Categories.Create;

public sealed class CreateCategoryHandler : CommandHandler<CreateCategoryCommand, Guid>
{
    private readonly IRepository<Category> _repo;
    private readonly IIdGenerator _ids;
    private readonly IIntegrationEventPublisher _events;

    public CreateCategoryHandler(IRepository<Category> repo, IUnitOfWork uow, IIdGenerator ids, IIntegrationEventPublisher events) : base(uow)
    {
        _repo = repo;
        _ids = ids;
        _events = events;
    }

    public override async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var name = request.Name?.Trim() ?? string.Empty;

        var exists = await _repo.ExistsAsync(c => c.Name == name, ct);
        if (exists) return Fail("Category with same name exists.");

        var id = _ids.NewGuid();
        var entity = new Category(id, name, request.Description);

        await _repo.AddAsync(entity, ct);
        await Uow.SaveChangesAsync(ct);
        await _events.PublishAsync(new CategoryCreated(entity.Id, entity.Name), topic: "content.category.created", ct: ct);
        return Ok(entity.Id);
    }
}
