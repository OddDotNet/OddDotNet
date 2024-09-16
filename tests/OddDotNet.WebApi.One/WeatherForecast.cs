namespace OddDotNet.WebApi.One;

public class WeatherForecast
{
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public required string Summary { get; set; }

    public override string ToString()
    {
        return $"WeatherForecast: {Id} {Date} {TemperatureC} {TemperatureF} {Summary ?? ""}";
    }
}