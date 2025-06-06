using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using DevHabit.Api.DTOs.Auth;

using DevHabit.Api.DTOs.Users;
using DevHabit.Api.Settings;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DevHabit.Api.Services;

public class TokenProviderService(IOptions<JwtAuthenticationOptions> options)
{
    private readonly JwtAuthenticationOptions _options = options.Value;

    public AccessTokensDto Create(TokenRequest request)
    {
        return new AccessTokensDto(
            GenerateAccessToken(request),
            GenerateRefreshToken()
        );
    }

    public string GenerateAccessToken(TokenRequest request)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId),
            new(JwtRegisteredClaimNames.Email, request.Email),
        };
        var roleClaims = request.Roles.Select(r => new Claim(ClaimTypes.Role, r));
        claims.AddRange(roleClaims);

        var descriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationInMinutes),
            SigningCredentials = creds,
            Issuer = _options.Issuer,
            Audience = _options.Audience
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor) ?? throw new InvalidOperationException("Could not generate access token");

        return token;
    }

    public static string GenerateRefreshToken()
    {
        var random = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(random);

        return token;
    }

}