using DevHabit.Api.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public sealed class HabitConfiguration : IEntityTypeConfiguration<Entities.Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasMaxLength(500);

        builder.Property(h => h.Name).HasMaxLength(500);
        builder.Property(h => h.Description).HasMaxLength(500);

        // Todo: what does ownsOne do here?
        builder.OwnsOne(h => h.Frequency);

        builder.OwnsOne(h => h.Target, t => t.Property(t => t.Unit).HasMaxLength(50));

        builder.OwnsOne(h => h.Milestone);
    }
}