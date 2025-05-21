using System.Net;

using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;


namespace DevHabit.Api.Controllers;

[ApiController]
[Route("/auth")]
[AllowAnonymous]
public class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationDbContext appDbContext,
    IdentityDbContext identityDbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Register(RegisterUserDto dto)
    {
        var identityUser = new IdentityUser()
        {
            Email = dto.Email,
            UserName = dto.Email
        };

        var registrationResult = await userManager.CreateAsync(identityUser);

        if (!registrationResult.Succeeded)
        {
            var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: StatusCodes.Status400BadRequest);
            foreach (var error in registrationResult.Errors)
            {
                problem.Extensions.Add(error.Code, error.Description);

            }
            return BadRequest(problem);
        }
        return Ok();
    }


}