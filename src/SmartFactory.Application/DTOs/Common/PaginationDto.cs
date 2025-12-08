namespace SmartFactory.Application.DTOs.Common;

/// <summary>
/// Pagination parameters for queries.
/// </summary>
public record PaginationDto
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }

    public int Skip => (PageNumber - 1) * PageSize;

    public static PaginationDto Default => new();
    public static PaginationDto All => new() { PageSize = int.MaxValue };
}
