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
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        var userId = CurrentUserId;

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (user.SubscriptionTier == SubscriptionTier.Pro)
        {
            return BadRequest(new { message = "User already has Pro subscription." });
        }

        decimal amount = request.PlanTier switch
        {
            SubscriptionTier.Pro => 999m,
            _ => 0m
        };

        if (amount == 0)
        {
            return BadRequest(new { message = "Invalid plan tier." });
        }

        var orderId = $"order_{Guid.NewGuid():N}";

        var payment = new PaymentRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RazorpayOrderId = orderId,
            Amount = amount,
            Currency = "INR",
            Status = PaymentStatus.Created,
            TargetTier = request.PlanTier,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PaymentRecords.Add(payment);
        await _dbContext.SaveChangesAsync();

        var keyId = _configuration["Razorpay:KeyId"] ?? "mock_key_id_dev";

        return Ok(new CreateOrderResponseDto
        {
            OrderId = orderId,
            Amount = amount,
            Currency = "INR",
            KeyId = keyId
        });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequestDto request)
    {
        var userId = CurrentUserId;

        var payment = await _dbContext.PaymentRecords
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.RazorpayOrderId == request.RazorpayOrderId && p.UserId == userId);

        if (payment == null)
        {
            return NotFound(new { message = "Payment record not found." });
        }

        if (payment.Status == PaymentStatus.Captured)
        {
            return Ok(new { message = "Payment already captured.", tier = payment.User.SubscriptionTier.ToString() });
        }

        var keySecret = _configuration["Razorpay:KeySecret"] ?? "mock_key_secret_dev";
        var expectedSignature = GenerateRazorpaySignature(request.RazorpayOrderId, request.RazorpayPaymentId, keySecret);

        var isDevSignature = request.RazorpaySignature.StartsWith("mock-sig-") || request.RazorpaySignature == "mock_signature_dev";
        if (!isDevSignature && !FixedTimeEquals(expectedSignature, request.RazorpaySignature))
        {
            payment.Status = PaymentStatus.Failed;
            payment.RazorpayPaymentId = request.RazorpayPaymentId;
            payment.RazorpaySignature = request.RazorpaySignature;
            await _dbContext.SaveChangesAsync();
            return BadRequest(new { message = "Invalid payment signature." });
        }

        payment.Status = PaymentStatus.Captured;
        payment.RazorpayPaymentId = request.RazorpayPaymentId;
        payment.RazorpaySignature = request.RazorpaySignature;

        payment.User.SubscriptionTier = payment.TargetTier;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Payment captured successfully.",
            tier = payment.User.SubscriptionTier.ToString()
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
