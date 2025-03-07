using System.ComponentModel.DataAnnotations;

namespace DevHabit.Api.Entities;

public sealed class Tag
{
    public required string Id { get; set; }

    [Required]
    [MinLength(3)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}