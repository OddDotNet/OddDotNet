using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace OddDotNet.WebApi.One;

public class WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options) : DbContext(options)
{
    public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applies all the configurations for entities.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}