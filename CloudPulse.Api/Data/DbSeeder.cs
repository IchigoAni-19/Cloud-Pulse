using CloudPulse.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudPulse.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.EnsureCreatedAsync();

            const string seedEmail = "harsh@test.com";
            var user = await context.Users.Include(u => u.Assets).FirstOrDefaultAsync(u => u.Email == seedEmail);

            if (user == null)
            {
                logger.LogInformation("Creating seed user: {Email}", seedEmail);
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = seedEmail,
                    PhoneNumber = "+919876543210",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    Role = UserRole.Engineer,
                    SubscriptionTier = SubscriptionTier.Pro,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            else
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
                user.SubscriptionTier = SubscriptionTier.Pro;
                await context.SaveChangesAsync();
            }

            var hasAssets = await context.CloudAssets.AnyAsync(ca => ca.UserId == user.Id);
            if (!hasAssets)
            {
                logger.LogInformation("Seeding cloud assets and 30-point telemetry health logs for user {Email}", seedEmail);

                var now = DateTime.UtcNow;
                var random = new Random(42);

                var assets = new List<CloudAsset>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Name = "Production API Gateway",
                        TargetUrl = "https://httpbin.org/status/200",
                        ResourceType = ResourceType.API,
                        Environment = EnvironmentType.Production,
                        CurrentStatus = AssetStatus.Healthy,
                        LastLatencyMs = 124,
                        LastCheckedAt = now.AddMinutes(-1),
                        CheckIntervalSeconds = 60,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Name = "Postgres Cluster Primary",
                        TargetUrl = "https://httpbin.org/status/200",
                        ResourceType = ResourceType.Database,
                        Environment = EnvironmentType.Production,
                        CurrentStatus = AssetStatus.Healthy,
                        LastLatencyMs = 42,
                        LastCheckedAt = now.AddMinutes(-2),
                        CheckIntervalSeconds = 60,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Name = "Redis Cache Staging",
                        TargetUrl = "https://httpbin.org/delay/2",
                        ResourceType = ResourceType.Database,
                        Environment = EnvironmentType.Staging,
                        CurrentStatus = AssetStatus.Degraded,
                        LastLatencyMs = 1940,
                        LastCheckedAt = now.AddMinutes(-1),
                        CheckIntervalSeconds = 60,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Name = "Staging Webhook Worker",
                        TargetUrl = "https://httpbin.org/status/500",
                        ResourceType = ResourceType.Worker,
                        Environment = EnvironmentType.Staging,
                        CurrentStatus = AssetStatus.Down,
                        LastLatencyMs = 310,
                        LastCheckedAt = now.AddMinutes(-3),
                        CheckIntervalSeconds = 60,
                        IsActive = true
                    }
                };

                context.CloudAssets.AddRange(assets);
                await context.SaveChangesAsync();

                var healthLogs = new List<AssetHealthLog>();

                foreach (var asset in assets)
                {
                    for (int i = 29; i >= 0; i--)
                    {
                        var timestamp = now.AddMinutes(-i * 45); // Spread 30 logs across past ~22.5 hours
                        int statusCode;
                        int latencyMs;
                        bool isSuccess;
                        string? errorMessage = null;

                        switch (asset.Name)
                        {
                            case "Production API Gateway":
                                // High availability, occasional latency fluctuation
                                if (i == 14)
                                {
                                    statusCode = 504;
                                    latencyMs = 380 + random.Next(20, 90);
                                    isSuccess = false;
                                    errorMessage = "Gateway Timeout";
                                }
                                else
                                {
                                    statusCode = 200;
                                    latencyMs = random.Next(75, 160);
                                    isSuccess = true;
                                }
                                break;

                            case "Postgres Cluster Primary":
                                // Ultra fast & 100% reliable
                                statusCode = 200;
                                latencyMs = random.Next(30, 65);
                                isSuccess = true;
                                break;

                            case "Redis Cache Staging":
                                // High latency / Degraded service
                                statusCode = 200;
                                latencyMs = random.Next(1400, 2200);
                                isSuccess = true;
                                break;

                            case "Staging Webhook Worker":
                                // Down / Failing service with 500 errors
                                statusCode = (i % 5 == 0) ? 502 : 500;
                                latencyMs = random.Next(240, 480);
                                isSuccess = false;
                                errorMessage = "Internal Server Error";
                                break;

                            default:
                                statusCode = 200;
                                latencyMs = random.Next(50, 150);
                                isSuccess = true;
                                break;
                        }

                        healthLogs.Add(new AssetHealthLog
                        {
                            CloudAssetId = asset.Id,
                            HttpStatusCode = statusCode,
                            LatencyMs = latencyMs,
                            IsSuccessful = isSuccess,
                            ErrorMessage = errorMessage,
                            CheckedAt = timestamp
                        });
                    }
                }

                context.AssetHealthLogs.AddRange(healthLogs);
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeded successfully with {AssetCount} assets and {LogCount} telemetry data points.", assets.Count, healthLogs.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during database seeding.");
        }
    }
}
