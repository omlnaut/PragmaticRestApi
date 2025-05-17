using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Net.Mime;

using Asp.Versioning;

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
[ApiVersion(1.0)]
[Route("/habits")]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.App.JsonV1,
    CustomMediaTypeNames.App.JsonV2,
    CustomMediaTypeNames.App.HateoasV1,
    CustomMediaTypeNames.App.HateoasV2
)]
public class HabitsController(ApplicationDbContext dbContext, LinkService linkService) : ControllerBase
{
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.App.HateoasV1)]
    public async Task<ActionResult> GetHabits([FromQuery] QueryParameters query,
                                                                   SortMappingProvider sortMappingProvider,
                                                                   DataShapingService dataShapingService)
    {
        query.Search ??= query.Search?.Trim().ToUpper(System.Globalization.CultureInfo.InvariantCulture);

        var sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter is not valid: {query.Sort}"
            );
        }

        if (!dataShapingService.Validate<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided fields parameter is not valid: {query.Fields}"
            );
        }

        var habitsQueryable = dbContext.Habits
            .Where(h => query.Search == null || h.Name.Contains(query.Search, StringComparison.InvariantCultureIgnoreCase)
                    || h.Description != null
                    && h.Description.Contains(query.Search, StringComparison.InvariantCultureIgnoreCase))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status)
            .ApplySort(query.Sort, sortMappings)
            .Select(HabitQueries.ToDto());

        var items = await habitsQueryable.Skip((query.Page - 1) * query.PageSize)
                               .Take(query.PageSize)
                               .ToListAsync();

        var totalCount = await habitsQueryable.CountAsync();

        var paginationItems = query.IncludeLinks
            ? dataShapingService.ShapeData(items, query.Fields, habitDto => CreateLinksForHabit(habitDto.Id, query.Fields))
            : dataShapingService.ShapeData(items, query.Fields, linkFactory: null);

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = paginationItems,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        };
        if (query.IncludeLinks)
        {
            paginationResult.Links = CreateLinksForHabits(query, paginationResult.HasNextPage, paginationResult.HasPreviousPage);
        }

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    [ApiVersion(1.0)]
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

    [HttpGet("{id}")]
    [ApiVersion(2.0)]
    public async Task<ActionResult> GetHabitByIdV2(string id,
                                                 string? fields,
                                                 DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<HabitWithTagsDtoV2>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided fields parameter is not valid: {fields}"
            );
        }

        var habit = await dbContext.Habits
            .Select(HabitQueries.ToHabitsWithTagsDtoV2())
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
            linkService.CreateLink(
                nameof(HabitTagsController.UpsertHabitTags),
                "upsert-tags",
                HttpMethods.Put,
                new{ habitId=id },
                HabitTagsController.Name),
        ];
    }

    private List<LinkDto> CreateLinksForHabits(QueryParameters query, bool hasNextPage, bool hasPreviousPage)
    {
        var links = new List<LinkDto>
        {
            linkService.CreateLink(nameof(GetHabits), "self", HttpMethods.Get,
            new {
                query.Page,
                 query.PageSize,
                 query.Fields,
                q = query.Search,
                 query.Type,
                 query.Status,
            }),
            linkService.CreateLink(nameof(GetHabits), "create", HttpMethods.Post)
        };

        if (hasNextPage)
        {
            links.Add(
                linkService.CreateLink(nameof(GetHabits), "next-page", HttpMethods.Get,
                new
                {
                    page = query.Page + 1,
                    query.PageSize,
                    query.Fields,
                    q = query.Search,
                    query.Type,
                    query.Status,
                })
            );
        }
        if (hasPreviousPage)
        {
            links.Add(
                linkService.CreateLink(nameof(GetHabits), "previous-page", HttpMethods.Get,
                new
                {
                    page = query.Page - 1,
                    query.PageSize,
                    query.Fields,
                    q = query.Search,
                    query.Type,
                    query.Status,
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