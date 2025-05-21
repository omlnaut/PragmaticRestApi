using DevHabit.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServices()
       .AddExceptionHandling()
       .AddObservability()
       .AddApplicationServices()
       .AddDatabase()
       .AddAuthenticationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.ApplyMigrationAsync();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.MapControllers();

await app.RunAsync().ConfigureAwait(false); // Change to false to avoid potential deadlocks
