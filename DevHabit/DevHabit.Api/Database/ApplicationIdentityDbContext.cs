using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DevHabit.Api.Entities;

namespace DevHabit.Api.Database;

public sealed class ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : IdentityDbContext(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Schemas.Identity);

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(e => e.Id);

            e.Property(e => e.UserId).HasMaxLength(300);
            e.Property(e => e.Token).HasMaxLength(1000);

            e.HasIndex(e => e.Token).IsUnique();

            e.HasOne(e => e.User)
             .WithMany()
             .HasForeignKey(e => e.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        }
        );
    }
}