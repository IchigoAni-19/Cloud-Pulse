using CloudPulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudPulse.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<CloudAsset> CloudAssets { get; set; }
    public DbSet<AssetHealthLog> AssetHealthLogs { get; set; }
    public DbSet<PaymentRecord> PaymentRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email)
                  .IsUnique();

            entity.HasIndex(u => u.GoogleSubjectId);

            entity.Property(u => u.Role)
                  .HasConversion<string>();

            entity.Property(u => u.SubscriptionTier)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<CloudAsset>(entity =>
        {
            entity.HasOne(ca => ca.User)
                  .WithMany(u => u.Assets)
                  .HasForeignKey(ca => ca.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(ca => ca.ResourceType)
                  .HasConversion<string>();

            entity.Property(ca => ca.Environment)
                  .HasConversion<string>();

            entity.Property(ca => ca.CurrentStatus)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<AssetHealthLog>(entity =>
        {
            entity.HasKey(ahl => ahl.Id);

            entity.Property(ahl => ahl.Id)
                  .ValueGeneratedOnAdd();

            entity.HasOne(ahl => ahl.CloudAsset)
                  .WithMany(ca => ca.HealthLogs)
                  .HasForeignKey(ahl => ahl.CloudAssetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ahl => ahl.CheckedAt);

            entity.HasIndex(ahl => new { ahl.CloudAssetId, ahl.CheckedAt });
        });

        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.HasOne(pr => pr.User)
                  .WithMany(u => u.Payments)
                  .HasForeignKey(pr => pr.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(pr => pr.Status)
                  .HasConversion<string>();

            entity.Property(pr => pr.TargetTier)
                  .HasConversion<string>();
        });
    }
}
