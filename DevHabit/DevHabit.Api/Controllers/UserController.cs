using System.Security.Claims;

using Asp.Versioning;

using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Extensions;
using DevHabit.Api.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.Member}")]
[ApiVersion(1.0)]
[Route("/users")]
public class UserController(ApplicationDbContext dbContext, UserContext userContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserById(string id)
    {
        var userIdFromRequest = await userContext.GetUserIdAsync();
        if (userIdFromRequest != id)
        {
            return Unauthorized();
        }

        var user = await dbContext.Users.Select(UserQueries.ToDto())
                                        .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"The requested user id is not known: {id}"
            );
        }

        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult> Me()
    {
        var userIdFromRequest = await userContext.GetUserIdAsync();

        if (userIdFromRequest is null)
        {
            return Unauthorized();
        }

        var dto = await dbContext.Users.Where(u => u.Id == userIdFromRequest)
                                       .Select(UserQueries.ToDto())
                                       .FirstOrDefaultAsync();

        if (dto is null)
        {
            return Unauthorized();
        }

        return Ok(dto);
    }
}