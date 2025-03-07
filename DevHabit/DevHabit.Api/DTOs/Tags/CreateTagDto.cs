using System.ComponentModel.DataAnnotations;

namespace DevHabit.Api.DTOs.Tags;

public sealed record CreateTagDto
{
    [Required]
    [MinLength(3)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }
}