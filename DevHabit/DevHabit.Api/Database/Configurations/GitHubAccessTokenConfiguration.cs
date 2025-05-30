using DevHabit.Api.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public sealed class GitHubAccessTokenConfiguration : IEntityTypeConfiguration<GitHubAccessToken>
{
    public void Configure(EntityTypeBuilder<GitHubAccessToken> builder)
    {
        builder.HasIndex(gh => gh.Id);

        builder.Property(gh => gh.Token).HasMaxLength(1000);

        builder.HasIndex(gh => gh.UserId).IsUnique();

        builder.HasOne<User>()
               .WithOne()
               .HasForeignKey<GitHubAccessToken>(gh => gh.UserId);
    }

}