namespace DevHabit.Api.DTOs.Common;

public sealed record PaginationResult<T> : ICollectionResponse<T>, ILinksResponse
{
    public required List<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public required List<LinkDto> Links { get; init; }
}
