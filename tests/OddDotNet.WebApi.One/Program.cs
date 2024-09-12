using Microsoft.EntityFrameworkCore;
using OddDotNet.WebApi.One;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WeatherForecastDbContext>(opt => opt.UseInMemoryDatabase("WeatherForecast"));
builder.Services.AddHealthChecks()
    .AddDbContextCheck<WeatherForecastDbContext>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<WeatherForecastDbContextInitializer>();

builder.AddServiceDefaults("OddDotNet.WebApi.One");

var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Initialize and seed database
    await using var scope = app.Services.CreateAsyncScope();
    var initializer = scope.ServiceProvider.GetRequiredService<WeatherForecastDbContextInitializer>();
    //initializer.Initialize();
    await initializer.SeedAsync();
    Console.WriteLine("Migrations complete");
}

app.MapGet("/weatherforecast", async (
        WeatherForecastDbContext weatherForecastDbContext, 
        ILogger<Program> logger) =>
{
    var weatherForecasts = await weatherForecastDbContext.WeatherForecasts
        .OrderBy(x => x.Summary)
        .ToArrayAsync();
    
    logger.LogInformation("Generated weather {COUNT} forecasts", weatherForecasts.Length);
    return weatherForecasts;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();
