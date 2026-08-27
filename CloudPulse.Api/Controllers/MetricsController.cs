using System.Security.Claims;
using CloudPulse.Api.Data;
using CloudPulse.Api.Dtos;
using CloudPulse.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudPulse.Api.Controllers;

[ApiController]
[Route("api/v1/metrics")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public MetricsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var userId = CurrentUserId;

        var assets = await _dbContext.CloudAssets
            .Where(ca => ca.UserId == userId)
            .ToListAsync();

        var totalAssets = assets.Count;
        var healthyCount = assets.Count(a => a.CurrentStatus == AssetStatus.Healthy);
        var degradedCount = assets.Count(a => a.CurrentStatus == AssetStatus.Degraded);
        var downCount = assets.Count(a => a.CurrentStatus == AssetStatus.Down);

        var assetIds = assets.Select(a => a.Id).ToList();

        var recentLogs = await _dbContext.AssetHealthLogs
            .Where(l => assetIds.Contains(l.CloudAssetId))
            .OrderByDescending(l => l.CheckedAt)
            .Take(500)
            .ToListAsync();

        double overallUptimePercentage = 0;
        double averageLatencyMs = 0;

        if (recentLogs.Any())
        {
            var totalChecks = recentLogs.Count;
            var successfulChecks = recentLogs.Count(l => l.IsSuccessful);
            overallUptimePercentage = totalChecks > 0 ? (double)successfulChecks / totalChecks * 100 : 0;
            averageLatencyMs = recentLogs.Average(l => l.LatencyMs);
        }

        return Ok(new DashboardSummaryDto
        {
            TotalAssets = totalAssets,
            HealthyCount = healthyCount,
            DegradedCount = degradedCount,
            DownCount = downCount,
            OverallUptimePercentage = Math.Round(overallUptimePercentage, 2),
            AverageLatencyMs = Math.Round(averageLatencyMs, 2)
        });
    }

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetAssetHistory(Guid id)
    {
        var userId = CurrentUserId;

        var asset = await _dbContext.CloudAssets
            .FirstOrDefaultAsync(ca => ca.Id == id && ca.UserId == userId);

        if (asset == null)
        {
            return NotFound(new { message = "Asset not found." });
        }

        var history = await _dbContext.AssetHealthLogs
            .Where(l => l.CloudAssetId == id)
            .OrderByDescending(l => l.CheckedAt)
            .Take(50)
            .OrderBy(l => l.CheckedAt)
            .ToListAsync();

        var historyDtos = history.Select(l => new HealthDataPointDto
        {
            Timestamp = l.CheckedAt,
            LatencyMs = l.LatencyMs,
            StatusCode = l.HttpStatusCode,
            IsSuccessful = l.IsSuccessful
        }).ToList();

        double uptimePercentage = 0;
        double avgLatencyMs = 0;

        if (history.Any())
        {
            var total = history.Count;
            var successful = history.Count(l => l.IsSuccessful);
            uptimePercentage = total > 0 ? (double)successful / total * 100 : 0;
            avgLatencyMs = history.Average(l => l.LatencyMs);
        }

        return Ok(new AssetMetricsHistoryDto
        {
            AssetId = id,
            UptimePercentage = Math.Round(uptimePercentage, 2),
            AvgLatencyMs = Math.Round(avgLatencyMs, 2),
            History = historyDtos
        });
    }
}
