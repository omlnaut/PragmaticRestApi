using Asp.Versioning;

using DevHabit.Api.Database;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("/users")]
public class UserController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetUserById(string id)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

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