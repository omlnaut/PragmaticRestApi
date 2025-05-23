using Asp.Versioning;

using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Middleware;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

using Newtonsoft.Json.Serialization;

using Npgsql;

using OpenTelemetry;

using OpenTelemetry.Metrics;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DevHabit.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;
        })
        .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
        .AddXmlSerializerFormatters();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();

        builder.Services.Configure<MvcOptions>(options =>
        {
            var formatter = options.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().First();
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.App.HateoasV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.App.HateoasV2);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.App.JsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.App.JsonV2);
        });
        builder.Services
            .AddApiVersioning(o =>
                {
                    o.DefaultApiVersion = new ApiVersion(1.0);
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ReportApiVersions = true;
                    o.ApiVersionSelector = new CurrentImplementationApiVersionSelector(o);
                    o.ApiVersionReader = ApiVersionReader.Combine(
                        new UrlSegmentApiVersionReader(),
                        new MediaTypeApiVersionReaderBuilder().Template("application/vnd.dev-habit.hateoas.{version}+json")
                                                              .Build());
                })
            .AddMvc();

        return builder;
    }

    public static WebApplicationBuilder AddExceptionHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(builder.Environment.ApplicationName)
            ).WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql()
            )
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
            )
            .UseOtlpExporter();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
        });
        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<HabitDto, Habit>>(_ => HabitMappings.SortMapping);

        builder.Services.AddTransient<SortMappingProvider>();
        builder.Services.AddTransient<DataShapingService>();
        builder.Services.AddTransient<LinkService>();

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Database"), npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
            .UseSnakeCaseNamingConvention();
        });
        return builder;
    }
}