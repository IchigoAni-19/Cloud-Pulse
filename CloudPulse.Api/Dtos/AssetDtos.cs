using System.ComponentModel.DataAnnotations;
using CloudPulse.Api.Models;

namespace CloudPulse.Api.Dtos;

public class CreateAssetRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string TargetUrl { get; set; } = string.Empty;

    [Required]
    public ResourceType ResourceType { get; set; }

    [Required]
    public EnvironmentType Environment { get; set; }

    public int CheckIntervalSeconds { get; set; } = 60;
}

public class UpdateAssetRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string TargetUrl { get; set; } = string.Empty;

    [Required]
    public ResourceType ResourceType { get; set; }

    [Required]
    public EnvironmentType Environment { get; set; }

    public bool IsActive { get; set; } = true;
}

public class AssetResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TargetUrl { get; set; } = string.Empty;

    public ResourceType ResourceType { get; set; }

    public EnvironmentType Environment { get; set; }

    public AssetStatus CurrentStatus { get; set; }

    public int LastLatencyMs { get; set; }

    public DateTime? LastCheckedAt { get; set; }

    public bool IsActive { get; set; }
}
