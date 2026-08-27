using System.Security.Claims;
using CloudPulse.Api.Data;
using CloudPulse.Api.Dtos;
using CloudPulse.Api.Models;
using CloudPulse.Api.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CloudPulse.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly TokenService _tokenService;
    private readonly OtpService _otpService;
    private readonly GoogleAuthSettings _googleAuth;

    public AuthController(AppDbContext dbContext, TokenService tokenService, OtpService otpService, IOptions<GoogleAuthSettings> googleAuth)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _otpService = otpService;
        _googleAuth = googleAuth.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters." });
        }

        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Email is already registered." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Engineer,
            SubscriptionTier = SubscriptionTier.Free,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var (token, expiresIn) = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresIn = expiresIn,
            User = MapToUserDto(user)
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || user.PasswordHash == null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var (token, expiresIn) = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresIn = expiresIn,
            User = MapToUserDto(user)
        });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return BadRequest(new { message = "Google ID token is required." });
        }

        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings();
            if (!string.IsNullOrEmpty(_googleAuth.ClientId) &&
                !_googleAuth.ClientId.StartsWith("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase))
            {
                validationSettings.Audience = new[] { _googleAuth.ClientId };
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.GoogleSubjectId == payload.Subject);
            if (user == null)
            {
                user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = payload.Email,
                        GoogleSubjectId = payload.Subject,
                        Role = UserRole.Engineer,
                        SubscriptionTier = SubscriptionTier.Free,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.Users.Add(user);
                }
                else
                {
                    user.GoogleSubjectId = payload.Subject;
                }

                await _dbContext.SaveChangesAsync();
            }

            var (token, expiresIn) = _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = token,
                ExpiresIn = expiresIn,
                User = MapToUserDto(user)
            });
        }
        catch (InvalidJwtException ex)
        {
            return BadRequest(new { message = $"Invalid Google ID token: {ex.Message}" });
        }
    }

    [HttpPost("phone/send-otp")]
    public IActionResult SendOtp([FromBody] SendOtpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(new { message = "Phone number is required." });
        }
        var otp = _otpService.GenerateOtp(request.PhoneNumber);
        return Ok(new { message = $"OTP sent (dev only: code is {otp}).", devOtp = otp });
    }

    [HttpPost("phone/verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.OtpCode))
        {
            return BadRequest(new { message = "Phone number and OTP code are required." });
        }

        if (!_otpService.VerifyOtp(request.PhoneNumber, request.OtpCode))
        {
            return BadRequest(new { message = "Invalid or expired OTP." });
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                PhoneNumber = request.PhoneNumber,
                Email = $"phone_{request.PhoneNumber}@local",
                Role = UserRole.Engineer,
                SubscriptionTier = SubscriptionTier.Free,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        var (token, expiresIn) = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresIn = expiresIn,
            User = MapToUserDto(user)
        });
    }

    private static UserDto MapToUserDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role,
        SubscriptionTier = user.SubscriptionTier
    };
}
