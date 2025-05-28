using System.Linq.Expressions;

using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Users;

internal static class UserQueries
{
    public static Expression<Func<User, UsersDto>> ToDto()
    {
        return user => new UsersDto
        {
            Id = user.IdentityId,
            Mail = user.Mail,
            Name = user.Mail,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc
        };
    }

}