namespace DevHabit.Api.DTOs.Users;

public sealed record UsersDto
{
    public required string Id { get; set; }
    public required string Mail { get; set; }
    public required string Name { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required DateTime? UpdatedAtUtc { get; set; }
}