using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FleetFuel.Data.Enums;

namespace FleetFuel.Data;

/// <summary>
/// Represents a user's subscription status and limits.
/// </summary>
public class UserSubscription
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;

    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public DateTime? TrialExpiresAt { get; set; }

    public DateTime? GracePeriodExpiresAt { get; set; }

    public DateTime? CurrentPeriodStart { get; set; }

    public DateTime? CurrentPeriodEnd { get; set; }

    // Usage limits for current period
    public int VehiclesUsed { get; set; }
    public int VehiclesLimit { get; set; } = 1;

    public int ReceiptsUsed { get; set; }
    public int ReceiptsLimit { get; set; } = 100;

    public bool HasOcr { get; set; } = false;

    public bool HasPdfExport { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}

/// <summary>
/// Extension methods for subscription checks.
/// </summary>
public static class SubscriptionExtensions
{
    public static bool CanAddVehicle(this UserSubscription subscription)
    {
        return subscription.VehiclesUsed < subscription.VehiclesLimit;
    }

    public static bool CanAddReceipt(this UserSubscription subscription)
    {
        return subscription.ReceiptsUsed < subscription.ReceiptsLimit;
    }

    public static double GetVehicleUsagePercent(this UserSubscription subscription)
    {
        if (subscription.VehiclesLimit == 0) return 100;
        return (double)subscription.VehiclesUsed / subscription.VehiclesLimit * 100;
    }

    public static double GetReceiptUsagePercent(this UserSubscription subscription)
    {
        if (subscription.ReceiptsLimit == 0) return 100;
        return (double)subscription.ReceiptsUsed / subscription.ReceiptsLimit * 100;
    }

    public static bool IsInGracePeriod(this UserSubscription subscription)
    {
        return subscription.Status == AccountStatus.GracePeriod 
            && subscription.GracePeriodExpiresAt > DateTime.UtcNow;
    }

    public static bool IsActive(this UserSubscription subscription)
    {
        return subscription.Status == AccountStatus.Active 
            || subscription.Status == AccountStatus.Trial 
            || IsInGracePeriod(subscription);
    }
}