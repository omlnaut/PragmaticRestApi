using System.Linq.Expressions;

using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

internal static class TagQueries
{
    public static Expression<Func<Tag, TagDto>> ToDto()
    {
        return tag => new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }
}
