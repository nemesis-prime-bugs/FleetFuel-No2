using FleetFuel.Api.Models;

namespace FleetFuel.Api.Services;

/// <summary>
/// Interface for subscription and billing operations.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Get current user's subscription info.
    /// </summary>
    Task<SubscriptionInfo?> GetSubscriptionAsync(Guid userId);

    /// <summary>
    /// Get usage statistics for current period.
    /// </summary>
    Task<UsageStats> GetUsageStatsAsync(Guid userId);

    /// <summary>
    /// Get all available subscription plans.
    /// </summary>
    Task<List<SubscriptionPlan>> GetPlansAsync();

    /// <summary>
    /// Create checkout session for subscription upgrade.
    /// </summary>
    Task<SubscriptionResponse> CreateCheckoutSessionAsync(Guid userId, SubscriptionTier newTier);

    /// <summary>
    /// Create customer portal session for billing management.
    /// </summary>
    Task<SubscriptionResponse> CreateCustomerPortalSessionAsync(Guid userId);

    /// <summary>
    /// Cancel subscription.
    /// </summary>
    Task<SubscriptionResponse> CancelSubscriptionAsync(Guid userId);
}