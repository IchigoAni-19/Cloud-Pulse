namespace CloudPulse.Api.Models;

public class AssetHealthLog
{
    public long Id { get; set; }

    public Guid CloudAssetId { get; set; }

    public CloudAsset CloudAsset { get; set; } = null!;

    public int HttpStatusCode { get; set; }

    public int LatencyMs { get; set; }

    public bool IsSuccessful { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
