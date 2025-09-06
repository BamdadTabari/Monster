using Monster.BuildingBlocks;

namespace Monster.Testing.Fakes;

public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    private DateTime _utcNow;

    DateTime IDateTimeProvider.UtcNow => DateTime.UtcNow;

    public FakeDateTimeProvider(DateTime? fixedUtcNow = null)
        => _utcNow = fixedUtcNow ?? new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public DateTime UtcNow() => _utcNow;
    public void Advance(TimeSpan by) => _utcNow = _utcNow.Add(by);
    public void Set(DateTime utcNow) => _utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
}
