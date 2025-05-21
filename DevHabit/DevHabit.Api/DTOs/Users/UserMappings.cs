using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto, string identityId)
    {
        return new User()
        {
            Id = $"u_{Guid.NewGuid()}",
            Mail = dto.Email,
            Name = dto.Name,
            CreatedAtUtc = DateTime.UtcNow,
            IdentityId = identityId
        };
    }
}