using DevHabit.Api.Entities;
using DevHabit.Api.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace DevHabit.Api.Controllers;

public record AcceptHeaderDto
{
    [FromHeader(Name = "Accept")]
    public string? Accept { get; set; }

    public bool IncludeLinks =>
        MediaTypeHeaderValue.TryParse(Accept, out MediaTypeHeaderValue? mediaType)
        && mediaType.SubTypeWithoutSuffix.HasValue
        && mediaType.SubTypeWithoutSuffix.Value.Contains(CustomMediaTypeNames.App.HateoasBaseType, System.StringComparison.InvariantCultureIgnoreCase);
}

public sealed record QueryParameters : AcceptHeaderDto
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
