namespace API.Dto.Resource;

public class GetResourcesFilterQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _page = 1;

    public string? Search { get; set; }
    public Guid? OwnerId { get; set; }

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 10,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }
}