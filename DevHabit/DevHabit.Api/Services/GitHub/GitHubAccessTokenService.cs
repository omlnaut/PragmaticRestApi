using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DevHabit.Api.Database;
using DevHabit.Api.Entities;

using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Services.GitHub;

public class GitHubAccessTokenService(ApplicationDbContext applicationDbContext)
{
    public async Task StoreAsync(string userId, StoreGithubAccessTokenDto dto)
    {
        var dbToken = await GetToken(userId);

        if (dbToken is null)
        {
            var newToken = new GitHubAccessToken()
            {
                Id = $"gh_{Guid.NewGuid()}",
                UserId = userId,
                Token = dto.Token,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(dto.ExpiresInDays),
                CreatedAtUtc = DateTime.UtcNow
            };
            applicationDbContext.GitHubAccessTokens.Add(newToken);
        }
        else
        {
            dbToken.Token = dto.Token;
            dbToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(dto.ExpiresInDays);
            dbToken.CreatedAtUtc = DateTime.UtcNow;
        }

        await applicationDbContext.SaveChangesAsync();
    }

    public async Task<string?> GetAsync(string userId)
    {
        var tokenEntity = await GetToken(userId);
        return tokenEntity?.Token;
    }

    private async Task<GitHubAccessToken?> GetToken(string userId)
    {
        return await applicationDbContext
            .GitHubAccessTokens
            .FirstOrDefaultAsync(gh => gh.UserId == userId);
    }

}

public sealed record StoreGithubAccessTokenDto(string Token, int ExpiresInDays);