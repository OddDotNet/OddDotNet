using Microsoft.Extensions.Options;

namespace OddDotNet;

public class CacheCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CacheCleanupBackgroundService> _logger;

    public CacheCleanupBackgroundService(IServiceProvider services, ILogger<CacheCleanupBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var signalLists = scope.ServiceProvider.GetServices<ISignalList>().ToList();
        var oddSettings = scope.ServiceProvider.GetRequiredService<IOptions<OddSettings>>().Value;

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Pruning spans");
            foreach (var signalList in signalLists)
                signalList.Prune();
                
            await Task.Delay(TimeSpan.FromMilliseconds(oddSettings.Cache.CleanupInterval), stoppingToken);
        }
    }
}