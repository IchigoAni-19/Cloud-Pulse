using System.ComponentModel.DataAnnotations;

namespace CloudPulse.Api.Models;

public enum PaymentStatus
{
    Created,
    Captured,
    Failed
}

public class PaymentRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    public string RazorpayOrderId { get; set; } = string.Empty;

    public string? RazorpayPaymentId { get; set; }

    public string? RazorpaySignature { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "INR";

    public PaymentStatus Status { get; set; } = PaymentStatus.Created;

    public SubscriptionTier TargetTier { get; set; } = SubscriptionTier.Pro;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
