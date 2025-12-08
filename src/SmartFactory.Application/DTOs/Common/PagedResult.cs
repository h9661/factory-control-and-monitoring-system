namespace SmartFactory.Application.DTOs.Common;

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Empty => new()
    {
        Items = Enumerable.Empty<T>(),
        TotalCount = 0,
        PageNumber = 1,
        PageSize = 20
    };

    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public PagedResult<TDestination> Map<TDestination>(Func<T, TDestination> mapper)
    {
        return new PagedResult<TDestination>
        {
            Items = Items.Select(mapper),
            TotalCount = TotalCount,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
