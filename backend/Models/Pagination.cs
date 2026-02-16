namespace FleetFuel.Api.Models;

/// <summary>
/// Generic pagination parameters.
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = Math.Max(1, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    public int Skip => (Page - 1) * PageSize;
}

/// <summary>
/// Generic paginated response.
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Sorting parameters.
/// </summary>
public class SortParams
{
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
}

/// <summary>
/// Combined query parameters.
/// </summary>
public class QueryParams : PaginationParams, SortParams
{
    public string? Search { get; set; }
}

/// <summary>
/// Pagination extension methods.
/// </summary>
public static class PaginationExtensions
{
    public static PaginatedResult<T> ToPaginatedResult<T>(
        this IEnumerable<T> query,
        int totalItems,
        int page,
        int pageSize)
    {
        return new PaginatedResult<T>
        {
            Items = query.ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}

/// <summary>
/// API response wrapper for consistent responses.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public PaginationMetadata? Pagination { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Error(string message) => new() { Success = false, Error = message };
    public static ApiResponse<T> Ok(T data, PaginationMetadata pagination) =>
        new() { Success = true, Data = data, Pagination = pagination };
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
