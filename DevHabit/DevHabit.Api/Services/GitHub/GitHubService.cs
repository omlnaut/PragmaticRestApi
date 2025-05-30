using System.Net.Http.Headers;

namespace DevHabit.Api.Services.GitHub;

public class GitHubService(IHttpClientFactory httpClientFactory, ILogger<GitHubService> logger)
{
    public async Task<string?> GetUserProfileAsync(string accessToken)
    {
        var client = GetHttpClient(accessToken);

        try
        {
            var uri = new Uri("user", UriKind.Relative);
            var response = await client.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get GitHub user profile");
            return null;
        }
    }

    private HttpClient GetHttpClient(string accessToken)
    {
        var client = httpClientFactory.CreateClient("github");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }
}