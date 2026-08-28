using System.ComponentModel.DataAnnotations;
using CloudPulse.Api.Models;

namespace CloudPulse.Api.Dtos;

public class CreateOrderRequestDto
{
    [Required]
    public SubscriptionTier PlanTier { get; set; } = SubscriptionTier.Pro;

    public string? BillingCycle { get; set; } = "Monthly";
}

public class CreateOrderResponseDto
{
    public string OrderId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "INR";

    public string KeyId { get; set; } = string.Empty;

    public bool IsServerOrder { get; set; } = false;
}

public class VerifyPaymentRequestDto
{
    [Required]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [Required]
    public string RazorpayPaymentId { get; set; } = string.Empty;

    [Required]
    public string RazorpaySignature { get; set; } = string.Empty;

    public string? PaymentMethod { get; set; } = null;

    public string? BillingName { get; set; } = null;

    public string? BillingEmail { get; set; } = null;

    public string? BillingPostalCode { get; set; } = null;

    public string? BillingCountry { get; set; } = null;
}

public class InvoiceDto
{
    public Guid Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public string OrderId { get; set; } = string.Empty;

    public string? PaymentId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "INR";

    public string Status { get; set; } = "Paid";

    public string PlanName { get; set; } = "CloudPulse Pro";

    public string? PaymentMethod { get; set; }

    public DateTime IssuedAt { get; set; }
}
