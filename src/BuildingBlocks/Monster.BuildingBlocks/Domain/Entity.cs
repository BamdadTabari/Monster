namespace Monster.BuildingBlocks.Domain;

/// <summary>Base entity with Id and domain-events support.</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();

    protected void Raise(IDomainEvent @event) => _events.Add(@event);
    public void ClearDomainEvents() => _events.Clear();
}
