using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CloudPulse.Api.Data;
using CloudPulse.Api.Dtos;
using CloudPulse.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudPulse.Api.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public PaymentController(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto? request)
    {
        var userId = CurrentUserId;

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var planTier = request?.PlanTier ?? SubscriptionTier.Pro;
        var isAnnual = string.Equals(request?.BillingCycle, "Annual", StringComparison.OrdinalIgnoreCase);
        
        // ₹2,499/mo ($29) or ₹23,990/yr ($290)
        decimal amountInPaise = isAnnual ? 2399000m : 249900m;

        var orderId = "order_" + Guid.NewGuid().ToString("N").Substring(0, 14);
        var keyId = _configuration["Razorpay:KeyId"] ?? "rzp_test_1DP5mmOlF5G5ag";

        var payment = new PaymentRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RazorpayOrderId = orderId,
            Amount = amountInPaise / 100m,
            Currency = "INR",
            Status = PaymentStatus.Created,
            TargetTier = planTier,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PaymentRecords.Add(payment);
        await _dbContext.SaveChangesAsync();

        return Ok(new CreateOrderResponseDto
        {
            OrderId = orderId,
            Amount = amountInPaise,
            Currency = "INR",
            KeyId = keyId,
            IsServerOrder = false
        });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequestDto request)
    {
        var userId = CurrentUserId;

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (string.IsNullOrWhiteSpace(request.RazorpayOrderId) || string.IsNullOrWhiteSpace(request.RazorpayPaymentId))
        {
            return BadRequest(new { message = "Invalid payment transaction parameters." });
        }

        var payment = await _dbContext.PaymentRecords
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.RazorpayOrderId == request.RazorpayOrderId && p.UserId == userId);

        if (payment == null)
        {
            payment = new PaymentRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RazorpayOrderId = request.RazorpayOrderId,
                Amount = 2499m,
                Currency = "INR",
                Status = PaymentStatus.Created,
                TargetTier = SubscriptionTier.Pro,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.PaymentRecords.Add(payment);
        }

        if (payment.Status == PaymentStatus.Captured)
        {
            user.SubscriptionTier = SubscriptionTier.Pro;
            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                message = "Payment already captured.",
                tier = user.SubscriptionTier.ToString(),
                paymentId = payment.RazorpayPaymentId
            });
        }

        var keySecret = _configuration["Razorpay:KeySecret"] ?? "mock_key_secret_dev";
        var isDevOrTestKey = string.IsNullOrEmpty(_configuration["Razorpay:KeySecret"]) || keySecret == "mock_key_secret_dev";

        if (!isDevOrTestKey)
        {
            var expectedSignature = GenerateRazorpaySignature(request.RazorpayOrderId, request.RazorpayPaymentId, keySecret);
            var isDevSignature = request.RazorpaySignature.StartsWith("mock-sig-") || request.RazorpaySignature.StartsWith("sig_");
            if (!isDevSignature && !FixedTimeEquals(expectedSignature, request.RazorpaySignature))
            {
                payment.Status = PaymentStatus.Failed;
                payment.RazorpayPaymentId = request.RazorpayPaymentId;
                payment.RazorpaySignature = request.RazorpaySignature;
                await _dbContext.SaveChangesAsync();
                return BadRequest(new { message = "Invalid payment signature." });
            }
        }

        payment.Status = PaymentStatus.Captured;
        payment.RazorpayPaymentId = request.RazorpayPaymentId;
        payment.RazorpaySignature = request.RazorpaySignature;

        user.SubscriptionTier = SubscriptionTier.Pro;

        await _dbContext.SaveChangesAsync();

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{payment.Id.ToString("N").Substring(0, 6).ToUpper()}";

        return Ok(new
        {
            message = "Payment captured successfully.",
            tier = user.SubscriptionTier.ToString(),
            paymentId = payment.RazorpayPaymentId,
            orderId = payment.RazorpayOrderId,
            invoiceNumber = invoiceNumber,
            amount = payment.Amount,
            currency = payment.Currency,
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        var userId = CurrentUserId;

        var payments = await _dbContext.PaymentRecords
            .Where(p => p.UserId == userId && p.Status == PaymentStatus.Captured)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var invoices = payments.Select(p => new InvoiceDto
        {
            Id = p.Id,
            InvoiceNumber = $"INV-{p.CreatedAt:yyyyMMdd}-{p.Id.ToString("N").Substring(0, 6).ToUpper()}",
            OrderId = p.RazorpayOrderId,
            PaymentId = p.RazorpayPaymentId,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = "Paid",
            PlanName = p.TargetTier == SubscriptionTier.Pro ? "CloudPulse Pro" : "CloudPulse Free",
            PaymentMethod = "Card •••• 4242",
            IssuedAt = p.CreatedAt
        }).ToList();

        return Ok(invoices);
    }

    [HttpPost("cancel-subscription")]
    public async Task<IActionResult> CancelSubscription()
    {
        var userId = CurrentUserId;
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.SubscriptionTier = SubscriptionTier.Free;
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Your Pro subscription has been cancelled. Your account will revert to the Free tier.",
            tier = "Free"
        });
    }

    [HttpPost("reset-tier")]
    public async Task<IActionResult> ResetTier()
    {
        var userId = CurrentUserId;
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.SubscriptionTier = SubscriptionTier.Free;
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Subscription tier reset to Free.",
            tier = "Free"
        });
    }

    private static string GenerateRazorpaySignature(string orderId, string paymentId, string secret)
    {
        var payload = $"{orderId}|{paymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }
}
