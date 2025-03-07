namespace DevHabit.Api.Controllers;

public sealed record UpsertHabitDto
{
    public List<string> TagIds { get; init; } = new();
}