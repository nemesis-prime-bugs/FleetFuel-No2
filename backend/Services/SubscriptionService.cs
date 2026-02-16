using FleetFuel.Data;
using FleetFuel.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service interface for subscription management and limit enforcement.
/// </summary>
public interface ISubscriptionService
{
    Task<UserSubscription?> GetSubscriptionAsync(Guid userId);
    Task<UserSubscription> GetOrCreateSubscriptionAsync(Guid userId);
    Task<bool> CanAddVehicleAsync(Guid userId);
    Task<bool> CanAddReceiptAsync(Guid userId);
    Task<UsageWarningLevel> GetVehicleUsageWarningLevelAsync(Guid userId);
    Task<UsageWarningLevel> GetReceiptUsageWarningLevelAsync(Guid userId);
    Task IncrementVehicleCountAsync(Guid userId);
    Task IncrementReceiptCountAsync(Guid userId);
    Task DecrementVehicleCountAsync(Guid userId);
    Task DecrementReceiptCountAsync(Guid userId);
    Task UpdateTierAsync(Guid userId, SubscriptionTier tier);
    Task UpdateStatusAsync(Guid userId, AccountStatus status);
}

public enum UsageWarningLevel
{
    Normal = 0,
    Warning = 1, // 80%
    Critical = 2, // 100%
    Blocked = 3
}

/// <summary>
/// Service implementation for subscription management.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(FleetFuelDbContext context, ILogger<SubscriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserSubscription?> GetSubscriptionAsync(Guid userId)
    {
        return await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<UserSubscription> GetOrCreateSubscriptionAsync(Guid userId)
    {
        var subscription = await GetSubscriptionAsync(userId);
        
        if (subscription == null)
        {
            subscription = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Tier = SubscriptionTier.Free,
                Status = AccountStatus.Trial,
                TrialExpiresAt = DateTime.UtcNow.AddDays(14),
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddYears(1),
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created trial subscription for user {UserId}", userId);
        }

        return subscription;
    }

    public async Task<bool> CanAddVehicleAsync(Guid userId)
    {
        var subscription = await GetOrCreateSubscriptionAsync(userId);
        return subscription.CanAddVehicle();
    }

    public async Task<bool> CanAddReceiptAsync(Guid userId)
    {
        var subscription = await GetOrCreateSubscriptionAsync(userId);
        return subscription.CanAddReceipt();
    }

    public async Task<UsageWarningLevel> GetVehicleUsageWarningLevelAsync(Guid userId)
    {
        var subscription = await GetOrCreateSubscriptionAsync(userId);
        var percent = subscription.GetVehicleUsagePercent();

        if (percent >= 100) return UsageWarningLevel.Blocked;
        if (percent >= 80) return UsageWarningLevel.Critical;
        if (percent >= 60) return UsageWarningLevel.Warning;
        return UsageWarningLevel.Normal;
    }

    public async Task<UsageWarningLevel> GetReceiptUsageWarningLevelAsync(Guid userId)
    {
        var subscription = await GetOrCreateSubscriptionAsync(userId);
        var percent = subscription.GetReceiptUsagePercent();

        if (percent >= 100) return UsageWarningLevel.Blocked;
        if (percent >= 80) return UsageWarningLevel.Critical;
        if (percent >= 60) return UsageWarningLevel.Warning;
        return UsageWarningLevel.Normal;
    }

    public async Task IncrementVehicleCountAsync(Guid userId)
    {
        var subscription = await GetOrCreateSubscriptionAsync(userId);
        subscription.VehiclesUsed++;
        subscription.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task IncrementReceiptCountAsync(Guid userId)
    {
        var subscription = await GetOrCreateSubscriptionAsync(userId);
        subscription.ReceiptsUsed++;
        subscription.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task DecrementVehicleCountAsync(Guid userId)
    {
        var subscription = await GetSubscriptionAsync(userId);
        if (subscription != null && subscription.VehiclesUsed > 0)
        {
            subscription.VehiclesUsed--;
            subscription.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DecrementReceiptCountAsync(Guid userId)
    {
        var subscription = await GetSubscriptionAsync(userId);
        if (subscription != null && subscription.ReceiptsUsed > 0)
        {
            subscription.ReceiptsUsed--;
            subscription.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateTierAsync(Guid userId, SubscriptionTier tier)
    {
        var subscription = await GetOrCreateSubscriptionAsync(userId);
        subscription.Tier = tier;
        subscription.ModifiedAt = DateTime.UtcNow;

        // Update limits based on tier
        switch (tier)
        {
            case SubscriptionTier.Free:
                subscription.VehiclesLimit = 1;
                subscription.ReceiptsLimit = 100;
                subscription.HasOcr = false;
                subscription.HasPdfExport = false;
                break;
            case SubscriptionTier.Pro:
                subscription.VehiclesLimit = 5;
                subscription.ReceiptsLimit = 1000;
                subscription.HasOcr = true;
                subscription.HasPdfExport = true;
                break;
            case SubscriptionTier.Enterprise:
                subscription.VehiclesLimit = 50;
                subscription.ReceiptsLimit = 10000;
                subscription.HasOcr = true;
                subscription.HasPdfExport = true;
                break;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated subscription tier for user {UserId} to {Tier}", userId, tier);
    }

    public async Task UpdateStatusAsync(Guid userId, AccountStatus status)
    {
        var subscription = await GetOrCreateSubscriptionAsync(userId);
        subscription.Status = status;
        subscription.ModifiedAt = DateTime.UtcNow;

        if (status == AccountStatus.GracePeriod)
        {
            subscription.GracePeriodExpiresAt = DateTime.UtcNow.AddDays(7);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated subscription status for user {UserId} to {Status}", userId, status);
    }
}