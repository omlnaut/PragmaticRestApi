using DevHabit.Api.Entities;
using DevHabit.Api.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

using Newtonsoft.Json;

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
    [JsonProperty(PropertyName = "search")]
    public string? Search { get; set; }

    [JsonProperty(PropertyName = "fields")]
    public string? Fields { get; set; }

    [JsonProperty(PropertyName = "type")]
    public HabitType? Type { get; set; }

    [JsonProperty(PropertyName = "status")]
    public HabitStatus? Status { get; set; }

    [JsonProperty(PropertyName = "sort")]
    public string? Sort { get; set; }

    [JsonProperty(PropertyName = "page")]
    public int Page { get; set; } = 1;

    [JsonProperty(PropertyName = "pageSize")]
    public int PageSize { get; set; } = 10;
}
