using HabitStatus = DevHabit.Api.Entities.HabitStatus;
using FrequencyType = DevHabit.Api.Entities.FrequencyType;
using DevHabit.Api.Entities;
using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.DTOs.Habits;
public sealed record HabitDto : ILinksResponse
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
    public required List<LinkDto> Links { get; set; }



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