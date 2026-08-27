using System.Collections.Concurrent;

namespace CloudPulse.Api.Services;

public class OtpService
{
    private readonly ConcurrentDictionary<string, (string Otp, DateTime ExpiresAt)> _otpCache = new();

    public string GenerateOtp(string phoneNumber)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        _otpCache[phoneNumber] = (otp, expiresAt);
        return otp;
    }

    public bool VerifyOtp(string phoneNumber, string otpCode)
    {
        if (!_otpCache.TryGetValue(phoneNumber, out var entry))
        {
            return false;
        }

        if (DateTime.UtcNow > entry.ExpiresAt)
        {
            _otpCache.TryRemove(phoneNumber, out _);
            return false;
        }

        if (entry.Otp != otpCode)
        {
            return false;
        }

        _otpCache.TryRemove(phoneNumber, out _);
        return true;
    }
}
