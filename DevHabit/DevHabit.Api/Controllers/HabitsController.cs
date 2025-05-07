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
public class HabitsController(ApplicationDbContext dbContext, LinkService linkService) : ControllerBase
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

        if (!dataShapingService.Validate<HabitDto>(query.fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided fields parameter is not valid: {query.fields}"
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
            Items = dataShapingService.ShapeData(items, query.fields, habitDto => CreateLinksForHabit(habitDto.Id, query.fields)),
            Page = query.page,
            PageSize = query.pageSize,
            TotalCount = totalCount,
        };
        paginationResult.Links = CreateLinksForHabits(query, paginationResult.HasNextPage, paginationResult.HasPreviousPage);

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetHabitById(string id,
                                                 string? fields,
                                                 DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<HabitWithTagsDto>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided fields parameter is not valid: {fields}"
            );
        }

        var habit = await dbContext.Habits
            .Select(HabitQueries.ToHabitsWithTagsDto())
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        var shaped = dataShapingService.ShapeData(habit, fields);
        List<LinkDto> links = CreateLinksForHabit(id, fields);
        shaped.TryAdd("link", links);

        return Ok(shaped);
    }

    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        return
        [
            linkService.CreateLink(nameof(GetHabitById), "self", HttpMethods.Get, new{id, fields}),
            linkService.CreateLink(nameof(UpdateHabit), "upsert", HttpMethods.Put, new{id}),
            linkService.CreateLink(nameof(DeleteHabit), "delete", HttpMethods.Delete, new{id}),

        ];
    }

    private List<LinkDto> CreateLinksForHabits(QueryParameters query, bool hasNextPage, bool hasPreviousPage)
    {
        var links = new List<LinkDto>
        {
            linkService.CreateLink(nameof(GetHabits), "self", HttpMethods.Get,
            new {
                query.page,
                 query.pageSize,
                 query.fields,
                q = query.search,
                 query.type,
                 query.status,
            }),
            linkService.CreateLink(nameof(GetHabits), "create", HttpMethods.Post)
        };

        if (hasNextPage)
        {
            links.Add(
                linkService.CreateLink(nameof(GetHabits), "next-page", HttpMethods.Get,
                new
                {
                    page = query.page + 1,
                    query.pageSize,
                    query.fields,
                    q = query.search,
                    query.type,
                    query.status,
                })
            );
        }
        if (hasPreviousPage)
        {
            links.Add(
                linkService.CreateLink(nameof(GetHabits), "previous-page", HttpMethods.Get,
                new
                {
                    page = query.page - 1,
                    query.pageSize,
                    query.fields,
                    q = query.search,
                    query.type,
                    query.status,
                })
            );
        }

        return links;
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);

        var habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var links = CreateLinksForHabit(habit.Id, null);
        var dto = habit.ToDto(links);

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

        var links = CreateLinksForHabit(habit.Id, null);
        var habitDto = habit.ToDto(links);
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