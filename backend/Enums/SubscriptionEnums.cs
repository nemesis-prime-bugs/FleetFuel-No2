namespace FleetFuel.Data.Enums;

/// <summary>
/// Subscription tier levels for FleetFuel.
/// </summary>
public enum SubscriptionTier
{
    Free = 0,
    Pro = 1,
    Enterprise = 2
}

/// <summary>
/// Account status for subscription management.
/// </summary>
public enum AccountStatus
{
    Trial = 0,
    Active = 1,
    GracePeriod = 2,
    PaymentFailed = 3,
    Suspended = 4,
    Cancelled = 5
}