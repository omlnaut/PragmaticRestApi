using System.Diagnostics.Tracing;

using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

internal static class HabitMappings
{
    public static Habit ToEntity(this CreateHabitDto dto)
    {
        var habit = new Habit()
        {
            Id = $"h_{Guid.NewGuid()}",
            Name = dto.Name,
            Description = dto.Description,
            Frequency = new Frequency
            {
                TimesPerPeriod = dto.Frequency.TimesPerPeriod,
                Type = dto.Frequency.Type
            },
            Target = new Target
            {
                Unit = dto.Target.Unit,
                Value = dto.Target.Value
            },
            Status = HabitStatus.Ongoing,
            IsArchived = false,
            EndDate = dto.EndDate,
            Milestone = dto.Milestone is not null ? new Milestone
            {
                Current = 0,
                Target = dto.Milestone.Target
            } : null,
            CreatedAtUtc = DateTime.UtcNow,
        };

        return habit;
    }


    public static HabitDto ToDto(this Habit habit)
    {
        var dto = new HabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Frequency = new FrequencyDto
            {
                TimesPerPeriod = habit.Frequency.TimesPerPeriod,
                Type = habit.Frequency.Type
            },
            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            Milestone = habit.Milestone == null ? null : new MilestoneDto
            {
                Target = habit.Milestone.Target,
                Current = habit.Milestone.Current
            },
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAt = habit.LastCompletedAt
        };

        return dto;
    }

}