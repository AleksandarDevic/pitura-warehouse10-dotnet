namespace SharedKernel;

public record PagedList<T>
{
    public PagedList(List<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public PagedList() { }

    public List<T> Items { get; set; } = [];

    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
}
