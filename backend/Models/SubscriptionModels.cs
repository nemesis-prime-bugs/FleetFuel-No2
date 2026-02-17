using System.ComponentModel.DataAnnotations;

namespace FleetFuel.Api.Models;

/// <summary>
/// Subscription tier enumeration.
/// </summary>
public enum SubscriptionTier
{
    Free = 0,
    Pro = 1,
    Business = 2
}

/// <summary>
/// Subscription plan details.
/// </summary>
public class SubscriptionPlan
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Interval { get; set; } = "month"; // month, year
    public List<string> Features { get; set; } = new();
    public int MaxVehicles { get; set; }
    public int MaxTripsPerMonth { get; set; }
}

/// <summary>
/// User subscription information.
/// </summary>
public class SubscriptionInfo
{
    public SubscriptionTier Tier { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public bool IsActive { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
}

/// <summary>
/// Usage statistics for the current billing period.
/// </summary>
public class UsageStats
{
    public int VehicleCount { get; set; }
    public int TripCountCurrentMonth { get; set; }
    public int ReceiptCountCurrentMonth { get; set; }
    public int VehicleLimit { get; set; }
    public int TripLimit { get; set; }
    public double VehicleUsagePercent => VehicleLimit > 0 ? (double)VehicleCount / VehicleLimit * 100 : 0;
    public double TripUsagePercent => TripLimit > 0 ? (double)TripCountCurrentMonth / TripLimit * 100 : 0;
}

/// <summary>
/// Request to change subscription tier.
/// </summary>
public class ChangeSubscriptionRequest
{
    [Required]
    public SubscriptionTier NewTier { get; set; }
}

/// <summary>
/// Response for subscription operations.
/// </summary>
public class SubscriptionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public SubscriptionInfo? Subscription { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? CustomerPortalUrl { get; set; }
}

/// <summary>
/// Available subscription plans (for frontend display).
/// </summary>
public static class SubscriptionPlans
{
    public static List<SubscriptionPlan> GetPlans()
    {
        return new List<SubscriptionPlan>
        {
            new SubscriptionPlan
            {
                Id = "free",
                Name = "Free",
                Price = 0,
                Currency = "USD",
                Interval = "month",
                Features = new List<string>
                {
                    "Up to 2 vehicles",
                    "Basic trip tracking",
                    "Basic fuel reports",
                    "30-day data history"
                },
                MaxVehicles = 2,
                MaxTripsPerMonth = 20
            },
            new SubscriptionPlan
            {
                Id = "pro",
                Name = "Pro",
                Price = 9.99m,
                Currency = "USD",
                Interval = "month",
                Features = new List<string>
                {
                    "Up to 10 vehicles",
                    "Advanced trip analytics",
                    "Unlimited fuel reports",
                    "Export data (CSV, JSON)",
                    "1-year data history",
                    "Priority support"
                },
                MaxVehicles = 10,
                MaxTripsPerMonth = 100
            },
            new SubscriptionPlan
            {
                Id = "business",
                Name = "Business",
                Price = 29.99m,
                Currency = "USD",
                Interval = "month",
                Features = new List<string>
                {
                    "Unlimited vehicles",
                    "Team collaboration",
                    "Advanced analytics",
                    "API access",
                    "Unlimited data history",
                    "Dedicated support",
                    "Custom integrations"
                },
                MaxVehicles = 999,
                MaxTripsPerMonth = 10000
            }
        };
    }
}