using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("seed")]
public class SeedController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> SeedDatabase()
    {
        // Check if database already has data (to prevent duplicates)
        bool hasData = await dbContext.Habits.AnyAsync() || await dbContext.Tags.AnyAsync();

        if (hasData)
        {
            return BadRequest("Database already contains data. Clear the database first if you want to reseed.");
        }

        // 1. Create tags
        var tagIds = await CreateTags();

        // 2. Create habits
        var habitIds = await CreateHabits();

        // 3. Create habit-tag relationships
        await CreateHabitTagRelationships(habitIds, tagIds);

        return Ok(new
        {
            message = "Database seeded successfully",
            tags = tagIds.Count,
            habits = habitIds.Count,
            relationships = habitIds.Count * 2 // Average number of tags per habit
        });
    }

    private async Task<Dictionary<string, string>> CreateTags()
    {
        var tagsData = new List<CreateTagDto>
        {
            new() { Name = "Health", Description = "Habits related to physical and mental wellbeing" },
            new() { Name = "Productivity", Description = "Habits that improve efficiency and output" },
            new() { Name = "Learning", Description = "Habits focused on acquiring new knowledge and skills" },
            new() { Name = "Personal", Description = "Personal development and self-improvement habits" },
            new() { Name = "Work", Description = "Professional development habits" },
            new() { Name = "Finance", Description = "Money management and financial habits" },
            new() { Name = "Social", Description = "Habits related to relationships and social skills" }
        };

        var tagIds = new Dictionary<string, string>();

        foreach (var tagData in tagsData)
        {
            var tag = tagData.ToEntity();
            dbContext.Tags.Add(tag);
            await dbContext.SaveChangesAsync();

            tagIds[tagData.Name] = tag.Id;
        }

        return tagIds;
    }

    private async Task<Dictionary<string, string>> CreateHabits()
    {
        var habitsData = new List<CreateHabitDto>
        {
            new() {
                Name = "Daily Exercise",
                Type = HabitType.Measurable,
                Description = "Exercise for at least 30 minutes every day",
                Frequency = new FrequencyDto {
                    TimesPerPeriod = 1,
                    Type = FrequencyType.Daily
                },
                Target = new TargetDto {
                    Value = 30,
                    Unit = "MINUTES"
                }
            },
            new() {
                Name = "Read Books",
                Type = HabitType.Measurable,
                Description = "Read at least 20 pages each day",
                Frequency = new FrequencyDto {
                    TimesPerPeriod = 1,
                    Type = FrequencyType.Daily
                },
                Target = new TargetDto {
                    Value = 20,
                    Unit = "PAGES"
                },
                Milestone = new MilestoneDto {
                    Target = 100,
                    Current = 0
                }
            },
            new() {
                Name = "Meditation",
                Type = HabitType.Measurable,
                Description = "Meditate for 10 minutes in the morning",
                Frequency = new FrequencyDto {
                    TimesPerPeriod = 1,
                    Type = FrequencyType.Daily
                },
                Target = new TargetDto {
                    Value = 10,
                    Unit = "MINUTES"
                }
            },
            new() {
                Name = "Weekly Planning",
                Type = HabitType.Binary,
                Description = "Plan tasks and goals for the week ahead",
                Frequency = new FrequencyDto {
                    TimesPerPeriod = 1,
                    Type = FrequencyType.Weekly
                },
                Target = new TargetDto {
                    Value = 1,
                    Unit = "SESSIONS"
                }
            },
            new() {
                Name = "Coding Practice",
                Type = HabitType.Measurable,
                Description = "Work on coding challenges or personal projects",
                Frequency = new FrequencyDto {
                    TimesPerPeriod = 1,
                    Type = FrequencyType.Daily
                },
                Target = new TargetDto {
                    Value = 45,
                    Unit = "MINUTES"
                }
            },
            new() {
                Name = "Budget Review",
                Type = HabitType.Binary,
                Description = "Review monthly budget and expenses",
                Frequency = new FrequencyDto {
                    TimesPerPeriod = 1,
                    Type = FrequencyType.Monthly
                },
                Target = new TargetDto {
                    Value = 1,
                    Unit = "TASKS"
                }
            },
            new() {
                Name = "Call Family",
                Type = HabitType.Binary,
                Description = "Call parents or siblings to stay connected",
                Frequency = new FrequencyDto {
                    TimesPerPeriod = 1,
                    Type = FrequencyType.Weekly
                },
                Target = new TargetDto {
                    Value = 1,
                    Unit = "TASKS"
                }
            }
        };

        var habitIds = new Dictionary<string, string>();

        foreach (var habitData in habitsData)
        {
            var habit = habitData.ToEntity();
            dbContext.Habits.Add(habit);
            await dbContext.SaveChangesAsync();

            habitIds[habitData.Name] = habit.Id;
        }

        return habitIds;
    }

    private async Task CreateHabitTagRelationships(Dictionary<string, string> habitIds, Dictionary<string, string> tagIds)
    {
        // Define relationships between habits and tags
        var relationships = new List<(string HabitName, List<string> TagNames)>
        {
            ("Daily Exercise", ["Health", "Personal"]),
            ("Read Books", ["Learning", "Personal"]),
            ("Meditation", ["Health", "Personal"]),
            ("Weekly Planning", ["Productivity", "Work"]),
            ("Coding Practice", ["Learning", "Work", "Productivity"]),
            ("Budget Review", ["Finance", "Personal"]),
            ("Call Family", ["Social", "Personal"])
        };

        foreach (var (habitName, tagNames) in relationships)
        {
            if (!habitIds.TryGetValue(habitName, out var habitId))
                continue;

            var tagIdsToConnect = tagNames
                .Where(tagName => tagIds.ContainsKey(tagName))
                .Select(tagName => tagIds[tagName])
                .ToList();

            var habitTagsToAdd = tagIdsToConnect.Select(tagId => new HabitTag
            {
                HabitId = habitId,
                TagId = tagId,
                CreatedAtUtc = DateTime.UtcNow
            });

            dbContext.HabitTags.AddRange(habitTagsToAdd);
            await dbContext.SaveChangesAsync();
        }
    }
}