using System.ComponentModel.DataAnnotations;

namespace CloudPulse.Api.Models;

public enum ResourceType
{
    API,
    Database,
    VM,
    Worker
}

public enum EnvironmentType
{
    Production,
    Staging,
    Development
}

public enum AssetStatus
{
    Healthy,
    Degraded,
    Down,
    Unknown
}

public class CloudAsset
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string TargetUrl { get; set; } = string.Empty;

    public ResourceType ResourceType { get; set; }

    public EnvironmentType Environment { get; set; }

    public AssetStatus CurrentStatus { get; set; } = AssetStatus.Unknown;

    public int LastLatencyMs { get; set; } = 0;

    public DateTime? LastCheckedAt { get; set; }

    public int CheckIntervalSeconds { get; set; } = 60;

    public bool IsActive { get; set; } = true;

    public ICollection<AssetHealthLog> HealthLogs { get; set; } = new List<AssetHealthLog>();
}
