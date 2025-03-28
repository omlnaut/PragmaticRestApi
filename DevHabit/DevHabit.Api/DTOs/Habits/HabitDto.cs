using HabitStatus = DevHabit.Api.Entities.HabitStatus;
using FrequencyType = DevHabit.Api.Entities.FrequencyType;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.DTOs.Habits;

public interface ICollectionResponse<T>
{
    public List<T> Items { get; init; }
}

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

public sealed record HabitWithTagsDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required HabitType Type { get; init; }
    public string? Description { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public HabitStatus Status { get; init; }
    public required bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? LastCompletedAt { get; init; }
    public required List<string> Tags { get; init; }

}
public sealed record HabitDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required HabitType Type { get; init; }
    public string? Description { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public HabitStatus Status { get; init; }
    public required bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? LastCompletedAt { get; init; }

}

public sealed record FrequencyDto
{
    public required int TimesPerPeriod { get; init; }

    public required FrequencyType Type { get; init; }
}
public sealed record TargetDto
{
    public required int Value { get; init; }
    public required string Unit { get; init; }
}
public sealed record MilestoneDto
{
    public required int Target { get; init; }
    public required int Current { get; init; }
}