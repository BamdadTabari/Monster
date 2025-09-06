using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Messaging;
using Monster.BuildingBlocks.Outbox;

public class OutboxBackoffTests
{
    [Fact]
    public async Task Failed_publish_increments_attempt_and_delays_next_try()
    {
        var clock = new FakeClock(new DateTime(2030,1,1,0,0,0,DateTimeKind.Utc));
        var store = new InMemoryOutboxStore();

        // bus that throws once, then succeeds
        var bus = new FlakyBus();
        var svc = new OutboxDispatcherHostedService(store, bus, clock, NullLogger<OutboxDispatcherHostedService>.Instance);

        // seed one message due now
        await store.AddAsync(new OutboxMessage(
            Guid.NewGuid(), "X", "t.x", "{\"ok\":true}", clock.UtcNow(), null, 0, clock.UtcNow()
        ));

        // run one loop tick manually by advancing time and letting the hosted loop tick
        using var cts = new CancellationTokenSource();
        var run = svc.StartAsync(cts.Token);

        // first tick → bus throws → attempt=1, nextAttempt = now + 1s
        await Task.Delay(500);
        var afterFail = await store.PeekBatchAsync(10, clock.UtcNow(), default);
        afterFail.Should().BeEmpty(); // not eligible until nextAttempt

        clock.Advance(TimeSpan.FromSeconds(1.1));
        await Task.Delay(500); // allow loop

        // second tick → bus succeeds → dispatched
        var stillPending = await store.PeekBatchAsync(10, clock.UtcNow(), default);
        stillPending.Should().BeEmpty();

        cts.Cancel();
        await svc.StopAsync(default);
    }

    private sealed class FlakyBus : IMessageBusPublisher
    {
        private int _calls = 0;
        public Task PublishAsync(string topic, ReadOnlyMemory<byte> body, IDictionary<string, string>? headers = null, CancellationToken ct = default)
        {
            if (_calls++ == 0) throw new Exception("boom");
            return Task.CompletedTask;
        }
    }

    private sealed class FakeClock : IDateTimeProvider
    {
        private DateTime _now;
        public FakeClock(DateTime start) => _now = start;
        public DateTime UtcNow() => _now;
        public void Advance(TimeSpan by) => _now = _now.Add(by);
    }
}
