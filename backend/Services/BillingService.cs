using FleetFuel.Api.Models;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Services;

/// <summary>
/// Interface for billing and checkout operations.
/// </summary>
public interface IBillingService
{
    Task<SubscriptionInfo?> GetSubscriptionInfoAsync(Guid userId);
    Task<UsageStats> GetUsageStatsAsync(Guid userId);
    Task<List<SubscriptionPlan>> GetPlansAsync();
    Task<SubscriptionResponse> CreateCheckoutSessionAsync(Guid userId, SubscriptionTier tier);
    Task<SubscriptionResponse> CreateCustomerPortalSessionAsync(Guid userId);
    Task<SubscriptionResponse> CancelSubscriptionAsync(Guid userId);
}

/// <summary>
/// Implementation of billing operations (Stripe integration ready).
/// For MVP, returns mock data and simulated checkout URLs.
/// </summary>
public class BillingService : IBillingService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<BillingService> _logger;

    public BillingService(FleetFuelDbContext context, ILogger<BillingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SubscriptionInfo?> GetSubscriptionInfoAsync(Guid userId)
    {
        var subscription = await _context.UserSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (subscription == null)
        {
            // Return default free tier
            return new SubscriptionInfo
            {
                Tier = SubscriptionTier.Free,
                IsActive = true
            };
        }

        return new SubscriptionInfo
        {
            Tier = subscription.Tier,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            IsActive = subscription.Status == Data.Enums.AccountStatus.Active,
            StripeCustomerId = subscription.StripeCustomerId,
            StripeSubscriptionId = subscription.StripeSubscriptionId
        };
    }

    public async Task<UsageStats> GetUsageStatsAsync(Guid userId)
    {
        var subscription = await _context.UserSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        var vehicleCount = await _context.Vehicles
            .AsNoTracking()
            .CountAsync(v => v.UserId == userId && !v.IsDeleted);

        var tripCount = await _context.Trips
            .AsNoTracking()
            .CountAsync(t => t.UserId == userId && !t.IsDeleted && 
                t.Date >= DateTime.UtcNow.AddDays(-30));

        var receiptCount = await _context.Receipts
            .AsNoTracking()
            .CountAsync(r => r.UserId == userId && !r.IsDeleted &&
                r.Date >= DateTime.UtcNow.AddDays(-30));

        int vehicleLimit = 2;
        int tripLimit = 20;

        if (subscription != null)
        {
            vehicleLimit = subscription.VehiclesLimit > 0 ? subscription.VehiclesLimit : 2;
            tripLimit = subscription.ReceiptsLimit > 0 ? Math.Min(subscription.ReceiptsLimit, 100) : 20;
        }

        return new UsageStats
        {
            VehicleCount = vehicleCount,
            TripCountCurrentMonth = tripCount,
            ReceiptCountCurrentMonth = receiptCount,
            VehicleLimit = vehicleLimit,
            TripLimit = tripLimit
        };
    }

    public Task<List<SubscriptionPlan>> GetPlansAsync()
    {
        return Task.FromResult(SubscriptionPlans.GetPlans());
    }

    public async Task<SubscriptionResponse> CreateCheckoutSessionAsync(Guid userId, SubscriptionTier newTier)
    {
        _logger.LogInformation("Creating checkout session for user {UserId}, tier {Tier}", userId, newTier);

        // For MVP, return mock checkout URL
        // In production, this would call Stripe API:
        // var session = await stripe.Checkout.Sessions.CreateAsync(...);
        
        var plan = SubscriptionPlans.GetPlans().FirstOrDefault(p => 
            (p.Id == "free" && newTier == SubscriptionTier.Free) ||
            (p.Id == "pro" && newTier == SubscriptionTier.Pro) ||
            (p.Id == "business" && newTier == SubscriptionTier.Business));

        return new SubscriptionResponse
        {
            Success = true,
            Message = $"Checkout session created for {plan?.Name ?? "Unknown"} plan",
            CheckoutUrl = $"/settings/billing/checkout?tier={newTier}",
            Subscription = await GetSubscriptionInfoAsync(userId)
        };
    }

    public async Task<SubscriptionResponse> CreateCustomerPortalSessionAsync(Guid userId)
    {
        _logger.LogInformation("Creating customer portal session for user {UserId}", userId);

        // For MVP, return mock portal URL
        // In production, this would call Stripe API:
        // var session = await stripe.BillingPortal.Sessions.CreateAsync(...);

        return new SubscriptionResponse
        {
            Success = true,
            Message = "Billing portal session created",
            CustomerPortalUrl = "/settings/billing/portal",
            Subscription = await GetSubscriptionInfoAsync(userId)
        };
    }

    public async Task<SubscriptionResponse> CancelSubscriptionAsync(Guid userId)
    {
        _logger.LogInformation("Canceling subscription for user {UserId}", userId);

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (subscription != null)
        {
            // Downgrade to free tier at end of period
            subscription.Tier = SubscriptionTier.Free;
            subscription.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return new SubscriptionResponse
        {
            Success = true,
            Message = "Subscription will be canceled at the end of the billing period",
            Subscription = await GetSubscriptionInfoAsync(userId)
        };
    }
}