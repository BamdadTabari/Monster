namespace Monster.BuildingBlocks;

public interface IDateTimeProvider { DateTime UtcNow(); }
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow() => DateTime.UtcNow;
}


public interface IIdGenerator { Guid NewGuid(); }
public sealed class GuidIdGenerator : IIdGenerator { public Guid NewGuid() => Guid.NewGuid(); }
