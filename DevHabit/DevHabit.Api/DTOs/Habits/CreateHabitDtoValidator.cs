using DevHabit.Api.Entities;

using FluentValidation;

namespace DevHabit.Api.DTOs.Habits;

public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits =
    [
        "MINUTES", "HOURS", "STEPS", "KM", "CAL", "PAGES", "BOOKS", "TASKS", "SESSIONS"
    ];

    private static readonly string[] AllowedUnitsForBinaryHabits =
    [
        "TASKS", "SESSIONS"
    ];
    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Habit name is required.")
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Habit name must be between 3 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null)
            .WithMessage("Description must be less than 500 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Habit type is required and must be a valid enum value.");

        RuleFor(x => x.Frequency.Type)
            .IsInEnum()
            .WithMessage("Frequency type is required and must be a valid enum value.");

        RuleFor(x => x.Frequency.TimesPerPeriod)
            .GreaterThan(0)
            .WithMessage("Frequency times per period must be greater than 0.");

        RuleFor(x => x.Target.Value)
            .GreaterThan(0)
            .WithMessage("Target must be greater than 0.");

        RuleFor(x => x.Target.Unit)
            .NotEmpty()
            .WithMessage("Target unit is required.")
            .Must(unit => AllowedUnits.Contains(unit))
            .WithMessage($"Target unit must be one of the following: {string.Join(", ", AllowedUnits)}.");

        RuleFor(x => x.EndDate)
            .Must(date => date is null || date.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must be in future");

        When(x => x.Milestone is not null, () =>
        {
            RuleFor(x => x.Milestone!.Target)
                .GreaterThan(0)
                .WithMessage("Milestone target must be greate than 0");
        });

        RuleFor(x => x.Target.Unit)
            .Must((dot, unit) => IsTargetUnitCompatibleWithType(dot.Type, unit))
            .WithMessage("nope");
    }

    private static bool IsTargetUnitCompatibleWithType(HabitType type, string unit)
    {
        var normalizedUnit = unit.ToUpperInvariant();

        return type switch
        {
            HabitType.Binary => AllowedUnitsForBinaryHabits.Contains(normalizedUnit),
            HabitType.Measurable => AllowedUnits.Contains(normalizedUnit),
            _ => false
        };
    }
}