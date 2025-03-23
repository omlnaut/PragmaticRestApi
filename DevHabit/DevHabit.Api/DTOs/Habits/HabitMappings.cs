using DevHabit.Api.Entities;
using DevHabit.Api.Services.Sorting;

namespace DevHabit.Api.DTOs.Habits;

internal static class HabitMappings
{
    public static readonly SortMappingDefinition<HabitDto, Habit> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(HabitDto.Name), nameof(Habit.Name)),
            new SortMapping(nameof(HabitDto.Type), nameof(Habit.Type)),
            new SortMapping(nameof(HabitDto.Description), nameof(Habit.Description)),
            new SortMapping(nameof(HabitDto.Status), nameof(Habit.Status)),
            new SortMapping(nameof(HabitDto.IsArchived), nameof(Habit.IsArchived)),
            new SortMapping(nameof(HabitDto.EndDate), nameof(Habit.EndDate)),
            new SortMapping(nameof(HabitDto.CreatedAtUtc), nameof(Habit.CreatedAtUtc)),
            new SortMapping(nameof(HabitDto.UpdatedAtUtc), nameof(Habit.UpdatedAtUtc)),
            new SortMapping(nameof(HabitDto.LastCompletedAt), nameof(Habit.LastCompletedAt)),
            
            // Frequency nested properties
            new SortMapping($"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.TimesPerPeriod)}", $"{nameof(Habit.Frequency)}.{nameof(Frequency.TimesPerPeriod)}"),
            new SortMapping($"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.Type)}", $"{nameof(Habit.Frequency)}.{nameof(Frequency.Type)}"),
            
            // Target nested properties
            new SortMapping($"{nameof(HabitDto.Target)}.{nameof(TargetDto.Value)}", $"{nameof(Habit.Target)}.{nameof(Target.Value)}"),
            new SortMapping($"{nameof(HabitDto.Target)}.{nameof(TargetDto.Unit)}", $"{nameof(Habit.Target)}.{nameof(Target.Unit)}"),
        ]
    };

    public static Habit ToEntity(this CreateHabitDto dto)
    {
        var habit = new Habit()
        {
            Id = $"h_{Guid.NewGuid()}",
            Name = dto.Name,
            Type = dto.Type,
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
            LastCompletedAt = habit.LastCompletedAt
        };

        return dto;
    }

    public static void UpdateFromDto(this Habit habit, UpdateHabitDto updateHabitDto)
    {
        habit.Name = updateHabitDto.Name;
        habit.Type = updateHabitDto.Type;
        habit.Description = updateHabitDto.Description;
        habit.EndDate = updateHabitDto.EndDate;

        habit.Frequency = new Frequency
        {
            TimesPerPeriod = updateHabitDto.Frequency.TimesPerPeriod,
            Type = updateHabitDto.Frequency.Type
        };

        habit.Target = new Target
        {
            Unit = updateHabitDto.Target.Unit,
            Value = updateHabitDto.Target.Value
        };

        if (updateHabitDto.Milestone is not null)
        {
            habit.Milestone ??= new Milestone();
            habit.Milestone.Target = updateHabitDto.Milestone.Target;
        }

        habit.UpdatedAtUtc = DateTime.UtcNow;
    }

}