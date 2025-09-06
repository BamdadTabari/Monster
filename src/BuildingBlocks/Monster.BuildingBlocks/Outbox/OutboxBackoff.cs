namespace Monster.BuildingBlocks.Outbox;

public static class OutboxBackoff
{
    // attempt: 0,1,2,3,>=4  → 1s,5s,30s,2m,10m
    public static TimeSpan ForAttempt(int attempt) => attempt switch
    {
        <= 0 => TimeSpan.FromSeconds(1),
        1    => TimeSpan.FromSeconds(5),
        2    => TimeSpan.FromSeconds(30),
        3    => TimeSpan.FromMinutes(2),
        _    => TimeSpan.FromMinutes(10)
    };
}
