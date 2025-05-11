using DevHabit.Api.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasMaxLength(150);

        builder.Property(u => u.Mail).HasMaxLength(150);
        builder.Property(u => u.IdentityId).HasMaxLength(500);

        builder.HasIndex(u => u.Mail).IsUnique();
        builder.HasIndex(u => u.IdentityId).IsUnique();
    }
}