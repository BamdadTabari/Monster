using Monster.BuildingBlocks; // where your IIdGenerator lives

namespace Content.Application.Tests.Fakes;

public sealed class FakeIdGenerator : IIdGenerator
{
    private readonly Guid _next;
    public FakeIdGenerator(Guid next) => _next = next;

    public Guid NewGuid() => _next;         // <- matches your interface
}
