namespace DevHabit.Api.Entities;

public sealed class GitHubAccessToken
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public required DateTime ExpiresAtUtc { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
}