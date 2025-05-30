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

using Microsoft.AspNetCore.Authorization;

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
[Authorize(Roles = $"{Roles.Member}")]
public class HabitsController(
    ApplicationDbContext dbContext,
    LinkService linkService,
    UserContext userContext) : ControllerBase
{
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json, CustomMediaTypeNames.App.HateoasV1)]
    public async Task<ActionResult> GetHabits([FromQuery] QueryParameters query,
                                                                   SortMappingProvider sortMappingProvider,
                                                                   DataShapingService dataShapingService)
    {
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

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
            .Where(h => h.UserId == userId)
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
                                                 GetHabitParameters query,
                                                 DataShapingService dataShapingService)
    {
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!dataShapingService.Validate<HabitWithTagsDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided query.Fields parameter is not valid: {query.Fields}"
            );
        }

        var habit = await dbContext.Habits
            .Where(h => h.UserId == userId)
            .Select(HabitQueries.ToHabitsWithTagsDto())
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        var shaped = dataShapingService.ShapeData(habit, query.Fields);

        if (query.IncludeLinks)
        {
            List<LinkDto> links = CreateLinksForHabit(id, query.Fields);
            shaped.TryAdd("link", links);
        }

        return Ok(shaped);
    }

    [HttpGet("{id}")]
    [ApiVersion(2.0)]
    public async Task<ActionResult> GetHabitByIdV2(string id,
                                                 GetHabitParameters query,
                                                 DataShapingService dataShapingService)
    {
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

#pragma warning disable S1481 // Unused local variables should be removed

        var versionFromRequest = HttpContext.GetRequestedApiVersion();

        var acceptHeader = Request.Headers.Accept.ToString();
#pragma warning restore S1481 // Unused local variables should be removed

        if (!dataShapingService.Validate<HabitWithTagsDtoV2>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided fields parameter is not valid: {query.Fields}"
            );
        }

        var habit = await dbContext.Habits
            .Where(h => h.UserId == userId)
            .Select(HabitQueries.ToHabitsWithTagsDtoV2())
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        var shaped = dataShapingService.ShapeData(habit, query.Fields);

        if (query.IncludeLinks)
        {
            List<LinkDto> links = CreateLinksForHabit(id, query.Fields);
            shaped.TryAdd("link", links);
        }

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

        var userId = await userContext.GetUserIdAsync();

        if (userId is null)
        {
            return Unauthorized();
        }

        var habit = createHabitDto.ToEntity(userId);

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var links = CreateLinksForHabit(habit.Id, null);
        var dto = habit.ToDto(links);

        return CreatedAtAction(nameof(GetHabitById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, [FromBody] UpdateHabitDto updateHabitDto)
    {
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        var habit = await dbContext.Habits
            .Where(h => h.UserId == userId)
            .FirstOrDefaultAsync(h => h.Id == id);

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
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        var habit = await dbContext.Habits
            .Where(h => h.UserId == userId)
            .FirstOrDefaultAsync(h => h.Id == id);

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
        var userId = await userContext.GetUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        var habit = await dbContext.Habits
            .Where(h => h.UserId == userId)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}