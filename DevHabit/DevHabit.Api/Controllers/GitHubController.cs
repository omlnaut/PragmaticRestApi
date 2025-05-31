using DevHabit.Api.Services;
using DevHabit.Api.Services.GitHub;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("github")]
[Authorize]
#pragma warning disable S6960 // Controllers should not have mixed responsibilities
public class GitHubController(
#pragma warning restore S6960 // Controllers should not have mixed responsibilities
    GitHubService gitHubService,
    GitHubAccessTokenService gitHubAccessTokenService,
    UserContext userContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> GetProfile()
    {
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        var accessToken = await gitHubAccessTokenService.GetAsync(userId);

        if (accessToken is null)
        {
            return Forbid();
        }

        var profileDto = await gitHubService.GetUserProfileAsync(accessToken);

        if (profileDto == null)
        {
            return NotFound("GitHub profile not found");
        }

        return Ok(profileDto);
    }

    [HttpPut("personal-access-token")]
    public async Task<ActionResult> StoreAccessToken(StoreGithubAccessTokenDto dto)
    {
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        await gitHubAccessTokenService.StoreAsync(userId, dto);

        return NoContent();
    }

    [HttpDelete("personal-access-token")]
    public async Task<ActionResult> RevokeAccessToken()
    {
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        await gitHubAccessTokenService.RevokeAsync(userId);

        return NoContent();
    }
}