using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace DevHabit.Api.Controllers;

[ApiController]
[Route("/auth")]
[AllowAnonymous]
public class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationDbContext appDbContext,
    ApplicationIdentityDbContext identityDbContext) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterUserDto dto)
    {
        var identityUser = new IdentityUser()
        {
            Email = dto.Email,
            UserName = dto.Email
        };

        using var transaction = await identityDbContext.Database.BeginTransactionAsync();
        appDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await appDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

        var registrationResult = await userManager.CreateAsync(identityUser, dto.Password);

        if (!registrationResult.Succeeded)
        {
            var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: StatusCodes.Status400BadRequest);
            foreach (var error in registrationResult.Errors)
            {
                problem.Extensions.Add(error.Code, error.Description);

            }
            return BadRequest(problem);
        }

        var user = dto.ToEntity(identityUser.Id);

        await appDbContext.Users.AddAsync(user);

        await appDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(user.Id);
    }


}