using System.Linq.Expressions;

using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits()
    {
        List<HabitDto> habits = await dbContext
            .Habits
            .Select(HabitQueries.ToDto())
            .ToListAsync();

        var habitsDto = new HabitsCollectionDto { Data = habits };

        return Ok(habitsDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabitById(string id)
    {
        var habit = await dbContext.Habits
            .Select(HabitQueries.ToDto())
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto)
    {
        var habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var dto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabitById), new { id = dto.Id }, dto);
    }
}