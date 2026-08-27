using System.ComponentModel.DataAnnotations;

namespace CloudPulse.Api.Models;

public enum UserRole
{
    Admin,
    Engineer
}

public enum SubscriptionTier
{
    Free,
    Pro
}

public class User
{
    public Guid Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? PasswordHash { get; set; }

    public string? GoogleSubjectId { get; set; }

    public UserRole Role { get; set; } = UserRole.Engineer;

    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CloudAsset> Assets { get; set; } = new List<CloudAsset>();

    public ICollection<PaymentRecord> Payments { get; set; } = new List<PaymentRecord>();
}
