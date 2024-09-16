namespace OddDotNet.WebApi.One;

public class WeatherForecastDbContextInitializer(
    WeatherForecastDbContext context,
    ILogger<WeatherForecastDbContext> logger)
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public void Initialize()
    {
        try
        {
            // if (context.Database.IsSqlServer())
            // {
            //     context.Database.Migrate();
            // }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        if (!context.WeatherForecasts.Any())
        {
            var rng = new Random();

            var weatherForecastsToSeed = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            foreach (var forecast in weatherForecastsToSeed)
            {
                context.WeatherForecasts.Add(forecast);
            }

            await context.SaveChangesAsync();

            logger.LogInformation("New WeatherForecasts were seeded: {NumberOFForecasts}", weatherForecastsToSeed.Length);
        }
        else
        {
            logger.LogInformation("No WeatherForecasts were seeded");
        }
    }
}