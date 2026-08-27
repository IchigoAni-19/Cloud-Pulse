using CloudPulse.Api.Data;
using CloudPulse.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudPulse.Api.Services;

public class HealthMonitoringWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthMonitoringWorker> _logger;

    public HealthMonitoringWorker(IServiceProvider serviceProvider, ILogger<HealthMonitoringWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HealthMonitoringWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunHealthChecksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running health checks.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("HealthMonitoringWorker is stopping.");
    }

    private async Task RunHealthChecksAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("HealthPoller");

        var activeAssets = await dbContext.CloudAssets
            .Where(ca => ca.IsActive)
            .ToListAsync(stoppingToken);

        _logger.LogInformation("Running health checks for {Count} active assets.", activeAssets.Count);

        foreach (var asset in activeAssets)
        {
            await CheckAssetHealthAsync(dbContext, httpClient, asset, stoppingToken);
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task CheckAssetHealthAsync(AppDbContext dbContext, HttpClient httpClient, CloudAsset asset, CancellationToken stoppingToken)
    {
        var log = new AssetHealthLog
        {
            CloudAssetId = asset.Id,
            CheckedAt = DateTime.UtcNow
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await httpClient.GetAsync(asset.TargetUrl, cts.Token);
            stopwatch.Stop();

            log.LatencyMs = (int)stopwatch.ElapsedMilliseconds;
            log.HttpStatusCode = (int)response.StatusCode;
            log.IsSuccessful = response.IsSuccessStatusCode;

            if (response.IsSuccessStatusCode && log.LatencyMs < 800)
            {
                asset.CurrentStatus = AssetStatus.Healthy;
            }
            else if (response.IsSuccessStatusCode && log.LatencyMs >= 800)
            {
                asset.CurrentStatus = AssetStatus.Degraded;
            }
            else if ((int)response.StatusCode >= 500)
            {
                asset.CurrentStatus = AssetStatus.Down;
                log.ErrorMessage = $"Server error: {response.StatusCode}";
            }
            else
            {
                asset.CurrentStatus = AssetStatus.Degraded;
                log.ErrorMessage = $"Client error: {response.StatusCode}";
            }
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            log.LatencyMs = (int)stopwatch.ElapsedMilliseconds;
            log.HttpStatusCode = 0;
            log.IsSuccessful = false;
            log.ErrorMessage = "Request timed out.";
            asset.CurrentStatus = AssetStatus.Down;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            log.LatencyMs = (int)stopwatch.ElapsedMilliseconds;
            log.HttpStatusCode = 0;
            log.IsSuccessful = false;
            log.ErrorMessage = ex.Message;
            asset.CurrentStatus = AssetStatus.Down;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            log.LatencyMs = (int)stopwatch.ElapsedMilliseconds;
            log.HttpStatusCode = 0;
            log.IsSuccessful = false;
            log.ErrorMessage = ex.Message;
            asset.CurrentStatus = AssetStatus.Unknown;
        }

        asset.LastLatencyMs = log.LatencyMs;
        asset.LastCheckedAt = log.CheckedAt;

        dbContext.AssetHealthLogs.Add(log);
    }
}
