using System.Linq.Expressions;

using DevHabit.Api.DTOs.Common;

using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

internal static class HabitQueries
{
    public static Expression<Func<Habit, HabitDto>> ToDto(List<LinkDto>? links = null)
    {
        return habit => new HabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Type = habit.Type,
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
            LastCompletedAt = habit.LastCompletedAt,
            Links = links ?? new List<LinkDto>(),
        };
    }
    public static Expression<Func<Habit, HabitWithTagsDto>> ToHabitsWithTagsDto()
    {
        return habit => new HabitWithTagsDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Type = habit.Type,
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
            LastCompletedAt = habit.LastCompletedAt,
            Tags = habit.Tags.Select(t => t.Name).ToList()
        };
    }

    public static Expression<Func<Habit, HabitWithTagsDtoV2>> ToHabitsWithTagsDtoV2()
    {
        return habit => new HabitWithTagsDtoV2
        {
            Id = habit.Id,
            Name = habit.Name,
            Type = habit.Type,
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
            CreatedAt = habit.CreatedAtUtc,
            UpdatedAt = habit.UpdatedAtUtc,
            LastCompletedAt = habit.LastCompletedAt,
            Tags = habit.Tags.Select(t => t.Name).ToList()
        };
    }
}
