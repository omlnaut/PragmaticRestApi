using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.DTOs.Common;

public sealed record PaginationResult<T> : ICollectionResponse<T>
{
    public required List<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static async Task<PaginationResult<T>> CreateAsync(IQueryable<T> query, int page, int pagesize)
    {
        var items = await query.Skip((page - 1) * pagesize)
                               .Take(pagesize)
                               .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PaginationResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pagesize,
            TotalCount = totalCount
        };
    }
}
