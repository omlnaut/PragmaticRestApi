namespace DevHabit.Api.Settings;

public sealed class JwtAuthenticationOptions
{
    public const string DefaultSectionName = "Jwt";
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string Key { get; set; }
    public required int ExpirationInMinutes { get; set; }
    public required int RefreshTokenExpirationDays { get; set; }

}