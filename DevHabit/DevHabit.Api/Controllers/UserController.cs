using Asp.Versioning;

using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("/users")]
public class UserController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserById(string id)
    {
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
}