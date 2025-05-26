using DevHabit.Api.DTOs.Users;

using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Auth;

public static class AuthMappings
{
    public static RefreshToken ToRefreshTokenEntity(
        this AccessTokensDto dto,
        string userId,
        int expirationDays)
    {
        var refreshToken = new RefreshToken()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = dto.RefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(expirationDays)
        };
        return refreshToken;
    }

}