using DevHabit.Api.Services.GitHub;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("github")]
[Authorize]
public class GitHubController(GitHubService gitHubService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> GetProfile(string accessToken)
    {

        var rawProfile = await gitHubService.GetUserProfileAsync(accessToken);

        return rawProfile ?? "nope";
    }
}