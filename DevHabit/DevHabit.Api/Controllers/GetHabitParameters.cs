using Newtonsoft.Json;

namespace DevHabit.Api.Controllers;

public sealed record GetHabitParameters : AcceptHeaderDto
{
    [JsonProperty(PropertyName = "fields")]
    public string? Fields { get; set; }
}