namespace Monster.BuildingBlocks;

public record PageRequest(int PageNumber = 1, int PageSize = 20, string? SortBy = null, bool Desc = false)
{
    public int Skip => Math.Max(0, (PageNumber - 1) * Math.Max(1, PageSize));
    public int Take => Math.Max(1, PageSize);
}

public record PageResponse<T>(IReadOnlyList<T> Items, long Total, int PageNumber, int PageSize);
