namespace Application.Common;

public class PaginationRequest
{
    public int PageNumber
    {
        get;
        set => field = value < 1 ? 1 : value;
    } = 1;

    public int PageSize
    {
        get;
        set => field = value switch
        {
            < 1 => 20,
            > 100 => 100, // Max 100 items per page
            _ => value
        };
    } = 20;

    public int Skip => (PageNumber - 1) * PageSize;
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public PaginatedResponse()
    {
    }

    public PaginatedResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
