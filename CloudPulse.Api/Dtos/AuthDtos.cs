using System.ComponentModel.DataAnnotations;
using CloudPulse.Api.Models;

namespace CloudPulse.Api.Dtos;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginRequestDto
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}

public class SendOtpRequestDto
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class VerifyOtpRequestDto
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string OtpCode { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }

    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserRole Role { get; set; }

    public SubscriptionTier SubscriptionTier { get; set; }
}
