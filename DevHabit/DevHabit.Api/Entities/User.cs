namespace DevHabit.Api.Entities;

public sealed class User
{
    public required string Id { get; set; }
    public required string Mail { get; set; }
    public required string Name { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public required string IdentityId { get; set; }
}