using System.Dynamic;
using System.Linq.Dynamic.Core;

using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;

using FluentValidation;

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetHabits([FromQuery] QueryParameters query,
                                                                   SortMappingProvider sortMappingProvider,
                                                                   DataShapingService dataShapingService)
    {
        query.search ??= query.search?.Trim().ToUpper(System.Globalization.CultureInfo.InvariantCulture);

        var sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter is not valid: {query.sort}"
            );
        }

        var habitsQueryable = dbContext.Habits
            .Where(h => query.search == null || h.Name.Contains(query.search, StringComparison.InvariantCultureIgnoreCase)
                    || h.Description != null
                    && h.Description.Contains(query.search, StringComparison.InvariantCultureIgnoreCase))
            .Where(h => query.type == null || h.Type == query.type)
            .Where(h => query.status == null || h.Status == query.status)
            .ApplySort(query.sort, sortMappings)
            .Select(HabitQueries.ToDto());

        var items = await habitsQueryable.Skip((query.page - 1) * query.pageSize)
                               .Take(query.pageSize)
                               .ToListAsync();

        var totalCount = await habitsQueryable.CountAsync();

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeData(items, query.fields),
            Page = query.page,
            PageSize = query.pageSize,
            TotalCount = totalCount
        };

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabitById(string id)
    {
        var habit = await dbContext.Habits
            .Select(HabitQueries.ToHabitsWithTagsDto())
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);

        var habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var dto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabitById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, [FromBody] UpdateHabitDto updateHabitDto)
    {

        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        var habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}