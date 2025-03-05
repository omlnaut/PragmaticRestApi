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

    public static void UpdateFromDto(this Tag tag, UpdateTagDto updateTagDto)
    {
        tag.Name = updateTagDto.Name;
        tag.Description = updateTagDto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }

}