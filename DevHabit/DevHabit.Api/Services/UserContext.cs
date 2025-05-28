using DevHabit.Api.Database;
using DevHabit.Api.Extensions;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Caching.Memory;

namespace DevHabit.Api.Services;

public class UserContext(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext applicationDbContext,
    IMemoryCache memoryCache)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
    private const string cacheKeyPrefix = "identityId";
    public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var identityIdFromRequest = httpContextAccessor.HttpContext!.User.GetUserId();

        var cacheKey = $"{cacheKeyPrefix}_{identityIdFromRequest}";

        var userId = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetSlidingExpiration(CacheDuration);
            var userId = await applicationDbContext.Users.Where(u => u.IdentityId == identityIdFromRequest)
                                                         .Select(u => u.Id)
                                                         .FirstOrDefaultAsync(cancellationToken);
            return userId;
        });

        return userId;
    }

}