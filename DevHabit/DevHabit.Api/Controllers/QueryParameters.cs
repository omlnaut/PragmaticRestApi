using DevHabit.Api.Entities;

using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.Controllers;

public sealed record QueryParameters
{
    [FromQuery(Name = "q")]
    public string? search { get; set; }
    public string? fields { get; set; }
    public HabitType? type { get; set; }
    public HabitStatus? status { get; set; }
    public string? sort { get; set; }

    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;

    [FromHeader(Name = "Accept")]
    public string? Accept { get; init; }
}
