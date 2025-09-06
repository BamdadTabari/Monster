using Monster.BuildingBlocks;

namespace Monster.Testing.Fakes;

public sealed class FakeIdGenerator : IIdGenerator
{
    private readonly Queue<Guid> _queue;

    public FakeIdGenerator(params Guid[] sequence)
        => _queue = new Queue<Guid>(sequence.Length == 0 ? new[] { Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee") } : sequence);

    public Guid NewGuid() => _queue.Count > 0 ? _queue.Dequeue() : Guid.Empty;
}
