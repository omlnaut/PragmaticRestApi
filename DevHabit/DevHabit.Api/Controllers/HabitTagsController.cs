using DevHabit.Api.Database;
using DevHabit.Api.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.Member}")]
[Route("habits/{habitId}/tags")]
public sealed class HabitTagsController(ApplicationDbContext dbContext) : ControllerBase
{
    public static readonly string Name = nameof(HabitTagsController).Replace("Controller", string.Empty, StringComparison.InvariantCulture);

    [HttpPut]
    public async Task<IActionResult> UpsertHabitTags(string habitId, UpsertHabitDto upsertHabitDto)
    {
        var habit = await dbContext.Habits
            .Include(h => h.HabitTags)
            .FirstOrDefaultAsync(h => h.Id == habitId);

        // habit not found
        if (habit is null)
        {
            return NotFound();
        }

        var currentTagIds = habit.HabitTags.Select(t => t.TagId).ToHashSet();

        // tags already matching
        if (currentTagIds.SetEquals(upsertHabitDto.TagIds))
        {
            return NoContent();
        }

        var dbTags = await dbContext.Tags
            .Where(t => upsertHabitDto.TagIds.Contains(t.Id))
            .ToListAsync();

        // unkown tag in dto
        if (dbTags.Count != upsertHabitDto.TagIds.Count)
        {
            return BadRequest("At least one dbtag was not valid");
        }

        // remove tags
        habit.HabitTags.RemoveAll(t => !upsertHabitDto.TagIds.Contains(t.TagId));

        // add tags
        var tagsToAdd = upsertHabitDto.TagIds.Except(currentTagIds);
        habit.HabitTags.AddRange(tagsToAdd.Select(ti => new HabitTag
        {
            HabitId = habit.Id,
            TagId = ti,
            CreatedAtUtc = DateTime.UtcNow
        }));

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{tagId}")]
    public async Task<IActionResult> DeleteHabitTags(string habitId, string tagId)
    {
        var habitTag = await dbContext.HabitTags.FirstOrDefaultAsync(h => h.HabitId == habitId && h.TagId == tagId);

        if (habitTag is null)
        {
            return NotFound();
        }

        dbContext.HabitTags.Remove(habitTag);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

}
