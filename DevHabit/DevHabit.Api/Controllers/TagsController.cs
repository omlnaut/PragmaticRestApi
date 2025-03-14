using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Tags;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("Tags")]
public class TagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags()
    {
        List<TagDto> tags = await dbContext
            .Tags
            .Select(TagQueries.ToDto())
            .ToListAsync();

        var habitsDto = new TagsCollectionDto { Data = tags };

        return Ok(habitsDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTagById(string id)
    {
        var tag = await dbContext.Tags
            .Select(TagQueries.ToDto())
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag is null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto, IValidator<CreateTagDto> validator)
    {
        // validate
        await validator.ValidateAndThrowAsync(createTagDto);

        // Check if the tag already exists
        var tag = createTagDto.ToEntity();

        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest);
            return BadRequest(problem);
        }

        dbContext.Tags.Add(tag);

        await dbContext.SaveChangesAsync();

        var dto = tag.ToDto();

        return CreatedAtAction(nameof(CreateTag), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, [FromBody] UpdateTagDto updateTagDto)
    {

        var tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);

        if (tag is null)
        {
            return NotFound();
        }

        tag.UpdateFromDto(updateTagDto);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        var tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);

        if (tag is null)
        {
            return NotFound();
        }

        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}