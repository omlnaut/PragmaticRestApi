using System.Diagnostics.Tracing;

using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

internal static class TagMappings
{
    public static Tag ToEntity(this CreateTagDto dto)
    {
        var tag = new Tag()
        {
            Id = $"t_{Guid.NewGuid()}",
            Name = dto.Name,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        return tag;
    }


    public static TagDto ToDto(this Tag habit)
    {
        var dto = new TagDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc
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