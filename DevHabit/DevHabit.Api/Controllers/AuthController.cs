using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Settings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;


namespace DevHabit.Api.Controllers;

[ApiController]
[Route("/auth")]
[AllowAnonymous]
public class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationDbContext appDbContext,
    ApplicationIdentityDbContext identityDbContext,
    TokenProviderService tokenProviderService,
    IOptions<JwtAuthenticationOptions> options) : ControllerBase
{
    private readonly JwtAuthenticationOptions _options = options.Value;


    [HttpPost("register")]
    public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto dto)
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

        var accessTokenDto = tokenProviderService.Create(new TokenRequest(identityUser.Id, identityUser.Email));
        var refreshToken = accessTokenDto.ToRefreshTokenEntity(identityUser.Id, _options.RefreshTokenExpirationDays);

        await identityDbContext.RefreshTokens.AddAsync(refreshToken);
        await identityDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(accessTokenDto);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginUserDto dto)
    {
        var identityUser = await userManager.FindByEmailAsync(dto.Email);

        if (identityUser is null || !await userManager.CheckPasswordAsync(identityUser, dto.Password))
        {
            return Unauthorized();
        }

        var tokenRequest = new TokenRequest(identityUser.Id, dto.Email);
        var tokens = tokenProviderService.Create(tokenRequest);

        var refreshToken = tokens.ToRefreshTokenEntity(identityUser.Id, _options.RefreshTokenExpirationDays);

        await identityDbContext.RefreshTokens.AddAsync(refreshToken);
        await identityDbContext.SaveChangesAsync();

        return Ok(tokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokensDto>> Refresh(RefreshTokenDto dto)
    {
        var dbToken = await identityDbContext.RefreshTokens.Include(r => r.User)
                                                     .FirstOrDefaultAsync(r => r.Token == dto.Token);
        if (dbToken is null)
        {
            return Unauthorized();
        }

        if (dbToken.ExpiresAtUtc < DateTime.UtcNow || dbToken.User is null || dbToken.User.Email is null)
        {
            return Unauthorized();
        }

        var request = new TokenRequest(dbToken.User.Id, dbToken.User.Email);
        var tokens = tokenProviderService.Create(request);

        var refreshToken = tokens.ToRefreshTokenEntity(dbToken.User.Id, _options.RefreshTokenExpirationDays);

        await identityDbContext.RefreshTokens.AddAsync(refreshToken);
        await identityDbContext.SaveChangesAsync();

        return Ok(tokens);
    }
}

public sealed record RefreshTokenDto(string Token);


public sealed record LoginUserDto(string Email, string Password);