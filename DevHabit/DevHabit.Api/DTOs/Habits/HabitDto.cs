using HabitStatus = DevHabit.Api.Entities.HabitStatus;
using FrequencyType = DevHabit.Api.Entities.FrequencyType;

namespace DevHabit.Api.DTOs.Habits;

public sealed record HabitDto
{
    public required string Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public HabitStatus Status { get; init; }
    public bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? LastCompletedAt { get; init; }

}

public sealed record FrequencyDto
{
    public int TimesPerPeriod { get; init; }

    public FrequencyType Type { get; init; }
}
public sealed record TargetDto
{
    public int Value { get; init; }
    public required string Unit { get; init; }
}
public sealed record MilestoneDto
{
    public int Target { get; init; }
    public int Current { get; init; }
}