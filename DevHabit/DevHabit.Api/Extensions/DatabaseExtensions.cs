using DevHabit.Api.Database;
using DevHabit.Api.Entities;

using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        using var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        try
        {
            await applicationDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Application Database migration completed.");

            await identityDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Identity Database migration completed.");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while migrating the databases.");
        }
    }

    public static async Task SeedRoles(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }

        if (!await roleManager.RoleExistsAsync(Roles.Member))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Member));
        }
    }

}