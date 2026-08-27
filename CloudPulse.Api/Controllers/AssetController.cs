using System.Security.Claims;
using CloudPulse.Api.Data;
using CloudPulse.Api.Dtos;
using CloudPulse.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudPulse.Api.Controllers;

[ApiController]
[Route("api/v1/assets")]
[Authorize]
public class AssetController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public AssetController(AppDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private SubscriptionTier CurrentSubscriptionTier => Enum.Parse<SubscriptionTier>(User.FindFirstValue("SubscriptionTier")!);

    [HttpGet]
    public async Task<IActionResult> GetAssets([FromQuery] EnvironmentType? env, [FromQuery] ResourceType? type)
    {
        var userId = CurrentUserId;
        var query = _dbContext.CloudAssets.Where(ca => ca.UserId == userId);

        if (env.HasValue)
        {
            query = query.Where(ca => ca.Environment == env.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(ca => ca.ResourceType == type.Value);
        }

        var assets = await query.ToListAsync();
        var dtos = assets.Select(MapToAssetResponseDto).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetRequestDto request)
    {
        var userId = CurrentUserId;
        var tier = CurrentSubscriptionTier;

        var currentCount = await _dbContext.CloudAssets.CountAsync(ca => ca.UserId == userId);
        if (tier == SubscriptionTier.Free && currentCount >= 3)
        {
            return BadRequest(new { message = "Free tier allows maximum 3 assets. Upgrade to Pro for unlimited." });
        }

        var asset = new CloudAsset
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            TargetUrl = request.TargetUrl,
            ResourceType = request.ResourceType,
            Environment = request.Environment,
            CheckIntervalSeconds = request.CheckIntervalSeconds > 0 ? request.CheckIntervalSeconds : 60,
            CurrentStatus = AssetStatus.Unknown,
            IsActive = true
        };

        _dbContext.CloudAssets.Add(asset);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssetById), new { id = asset.Id }, MapToAssetResponseDto(asset));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAssetById(Guid id)
    {
        var userId = CurrentUserId;
        var asset = await _dbContext.CloudAssets.FirstOrDefaultAsync(ca => ca.Id == id && ca.UserId == userId);

        if (asset == null)
        {
            return NotFound(new { message = "Asset not found." });
        }

        return Ok(MapToAssetResponseDto(asset));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsset(Guid id)
    {
        var userId = CurrentUserId;
        var asset = await _dbContext.CloudAssets.FirstOrDefaultAsync(ca => ca.Id == id && ca.UserId == userId);

        if (asset == null)
        {
            return NotFound(new { message = "Asset not found." });
        }

        _dbContext.CloudAssets.Remove(asset);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:guid}/ping")]
    public async Task<IActionResult> PingAsset(Guid id)
    {
        var userId = CurrentUserId;
        var asset = await _dbContext.CloudAssets.FirstOrDefaultAsync(ca => ca.Id == id && ca.UserId == userId);

        if (asset == null)
        {
            return NotFound(new { message = "Asset not found." });
        }

        var httpClient = _httpClientFactory.CreateClient("HealthPoller");
        var log = new AssetHealthLog
        {
            CloudAssetId = asset.Id,
            CheckedAt = DateTime.UtcNow
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await httpClient.GetAsync(asset.TargetUrl, cts.Token);
            stopwatch.Stop();

            log.LatencyMs = (int)stopwatch.ElapsedMilliseconds;
            log.HttpStatusCode = (int)response.StatusCode;
            log.IsSuccessful = response.IsSuccessStatusCode;

            if (response.IsSuccessStatusCode && log.LatencyMs < 800)
                asset.CurrentStatus = AssetStatus.Healthy;
            else if (response.IsSuccessStatusCode)
                asset.CurrentStatus = AssetStatus.Degraded;
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
            log.IsSuccessful = false;
            log.ErrorMessage = "Request timed out.";
            asset.CurrentStatus = AssetStatus.Down;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            log.LatencyMs = (int)stopwatch.ElapsedMilliseconds;
            log.IsSuccessful = false;
            log.ErrorMessage = ex.Message;
            asset.CurrentStatus = AssetStatus.Down;
        }

        asset.LastLatencyMs = log.LatencyMs;
        asset.LastCheckedAt = log.CheckedAt;

        _dbContext.AssetHealthLogs.Add(log);
        await _dbContext.SaveChangesAsync();

        return Ok(MapToAssetResponseDto(asset));
    }

    private static AssetResponseDto MapToAssetResponseDto(CloudAsset asset) => new()
    {
        Id = asset.Id,
        Name = asset.Name,
        TargetUrl = asset.TargetUrl,
        ResourceType = asset.ResourceType,
        Environment = asset.Environment,
        CurrentStatus = asset.CurrentStatus,
        LastLatencyMs = asset.LastLatencyMs,
        LastCheckedAt = asset.LastCheckedAt,
        IsActive = asset.IsActive
    };
}
