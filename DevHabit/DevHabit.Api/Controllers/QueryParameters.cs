using DevHabit.Api.Entities;

using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

public sealed record QueryParameters
{
    [FromQuery(Name = "q")]
    public string? search { get; set; }
    public HabitType? type { get; set; }
    public HabitStatus? status { get; set; }
    public string? sort { get; set; }

}
