using System.ComponentModel.DataAnnotations;
using CloudPulse.Api.Models;

namespace CloudPulse.Api.Dtos;

public class CreateOrderRequestDto
{
    [Required]
    public SubscriptionTier PlanTier { get; set; } = SubscriptionTier.Pro;
}

public class CreateOrderResponseDto
{
    public string OrderId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string KeyId { get; set; } = string.Empty;
}

public class VerifyPaymentRequestDto
{
    [Required]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [Required]
    public string RazorpayPaymentId { get; set; } = string.Empty;

    [Required]
    public string RazorpaySignature { get; set; } = string.Empty;
}
