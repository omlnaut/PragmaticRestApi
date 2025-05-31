using DevHabit.Api.DTOs.Common;

using Newtonsoft.Json;

namespace DevHabit.Api.Controllers;

public sealed record GithubProfileDto
{
    [JsonProperty("login")]
    public required string Login { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("avatar_url")]
    public required Uri AvatarUrl { get; set; }

    [JsonProperty("bio")]
    public string? Bio { get; set; }

    [JsonProperty("public_repos")]
    public int PublicRepos { get; set; }

    [JsonProperty("followers")]
    public int Followers { get; set; }

    [JsonProperty("following")]
    public int Following { get; set; }

    public required List<LinkDto> Links { get; init; } = [];
}
