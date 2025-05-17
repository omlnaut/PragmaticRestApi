using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DevHabit.Api.Database;

public sealed class ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : IdentityDbContext(options)
{

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schemas.Identity);
    }
}