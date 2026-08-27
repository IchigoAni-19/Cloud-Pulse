namespace CloudPulse.Api.Dtos;

public class DashboardSummaryDto
{
    public int TotalAssets { get; set; }

    public int HealthyCount { get; set; }

    public int DegradedCount { get; set; }

    public int DownCount { get; set; }

    public double OverallUptimePercentage { get; set; }

    public double AverageLatencyMs { get; set; }
}

public class AssetMetricsHistoryDto
{
    public Guid AssetId { get; set; }

    public double UptimePercentage { get; set; }

    public double AvgLatencyMs { get; set; }

    public List<HealthDataPointDto> History { get; set; } = new();
}

public class HealthDataPointDto
{
    public DateTime Timestamp { get; set; }

    public int LatencyMs { get; set; }

    public int StatusCode { get; set; }

    public bool IsSuccessful { get; set; }
}
