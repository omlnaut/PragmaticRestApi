using System.Net.Http.Headers;

using DevHabit.Api.Controllers;

using Newtonsoft.Json;

namespace DevHabit.Api.Services.GitHub;

public class GitHubService(IHttpClientFactory httpClientFactory, ILogger<GitHubService> logger)
{
    public async Task<GithubProfileDto?> GetUserProfileAsync(string accessToken)
    {
        var client = GetHttpClient(accessToken);

        try
        {
            var uri = new Uri("user", UriKind.Relative);
            var response = await client.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var rawProfile = await response.Content.ReadAsStringAsync();

            var profileDto = JsonConvert.DeserializeObject<GithubProfileDto>(rawProfile);

            return profileDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get GitHub user profile");
            return null;
        }
    }

    public async Task<string> GetUserEvents(string username, string accessToken)
    {
        var client = GetHttpClient(accessToken);

        try
        {
            var uri = new Uri($"users/{username}/events?per_page=100", UriKind.Relative);
            var response = await client.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var rawEvents = await response.Content.ReadAsStringAsync();

            return rawEvents;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get GitHub user profile");
            return string.Empty;
        }
    }

    private HttpClient GetHttpClient(string accessToken)
    {
        var client = httpClientFactory.CreateClient("github");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }
}