using System.Text;

using Asp.Versioning;

using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Middleware;
using DevHabit.Api.Services;
using DevHabit.Api.Services.GitHub;
using DevHabit.Api.Services.Sorting;
using DevHabit.Api.Settings;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;

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
        builder.Services
            .AddControllers(options =>
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
                    o.ApiVersionSelector = new DefaultApiVersionSelector(o);
                    o.ApiVersionReader = ApiVersionReader.Combine(
                        new MediaTypeApiVersionReaderBuilder().Template("application/vnd.dev-habit.hateoas.{version}+json").Build(),
                        new MediaTypeApiVersionReaderBuilder().Template("application/json;v={version}").Build()
                        );
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

        builder.Services.AddTransient<TokenProviderService>();

        builder.Services.AddScoped<UserContext>();
        builder.Services.AddMemoryCache(); builder.Services.AddTransient<GitHubService>();
        builder.Services.AddScoped<GitHubAccessTokenService>();

        _ = builder.Services.AddHttpClient("github").ConfigureHttpClient(c =>
        {

#pragma warning disable S1075 // URIs should not be hardcoded
            c.BaseAddress = new Uri("https://api.github.com");
#pragma warning restore S1075 // URIs should not be hardcoded
            c.DefaultRequestHeaders.UserAgent.Add(
                new System.Net.Http.Headers.ProductInfoHeaderValue("DevHabit", "1.0"));
            c.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        });

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
            .UseSnakeCaseNamingConvention();
        });

        builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
        {
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
            .UseSnakeCaseNamingConvention();
        });

        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                        .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        var jwtSection = builder.Configuration.GetSection(JwtAuthenticationOptions.DefaultSectionName);
        builder.Services.Configure<JwtAuthenticationOptions>(jwtSection);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtOptions = jwtSection.Get<JwtAuthenticationOptions>()
                                ?? throw new InvalidOperationException("JWT authentication options must be configured.");

                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });
        builder.Services.AddAuthorization();

        return builder;
    }
}