namespace DevHabit.Api.Entities;

public sealed class Habit
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public HabitType Type { get; set; }
    public string? Description { get; set; }
    public required Frequency Frequency { get; set; }
    public required Target Target { get; set; }
    public HabitStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public DateOnly? EndDate { get; set; }
    public Milestone? Milestone { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public List<HabitTag> HabitTags { get; init; } = [];
    public List<Tag> Tags { get; init; } = [];
}

public sealed class Milestone
{
    public int Target { get; set; }
    public int Current { get; set; }
}

public enum HabitStatus
{
    None = 0,
    Ongoing = 1,
    Completed = 2
}

public enum HabitType
{
    None = 0,
    Binary = 1,
    Measurable = 2
}

public sealed class Frequency
{
    public int TimesPerPeriod { get; set; }

    public FrequencyType Type { get; set; }
}

public enum FrequencyType
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}

public sealed class Target
{
    public int Value { get; set; }
    public required string Unit { get; set; }
}