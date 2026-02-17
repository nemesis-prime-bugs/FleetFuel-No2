using FleetFuel.Api.Models;
using FleetFuel.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetFuel.Api.Controllers;

/// <summary>
/// API controller for subscription and billing operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly IBillingService _billingService;

    public SubscriptionsController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    /// <summary>
    /// GET /api/v1/subscriptions/current
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentSubscription()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<SubscriptionInfo> { Error = "Unauthorized" });

        var subscription = await _billingService.GetSubscriptionInfoAsync(userId.Value);
        return Ok(new ApiResponse<SubscriptionInfo> { Success = true, Data = subscription });
    }

    /// <summary>
    /// GET /api/v1/subscriptions/usage
    /// </summary>
    [HttpGet("usage")]
    [ProducesResponseType(typeof(ApiResponse<UsageStats>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsageStats()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<UsageStats> { Error = "Unauthorized" });

        var usage = await _billingService.GetUsageStatsAsync(userId.Value);
        return Ok(new ApiResponse<UsageStats> { Success = true, Data = usage });
    }

    /// <summary>
    /// GET /api/v1/subscriptions/plans
    /// </summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(ApiResponse<List<SubscriptionPlan>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _billingService.GetPlansAsync();
        return Ok(new ApiResponse<List<SubscriptionPlan>> { Success = true, Data = plans });
    }

    /// <summary>
    /// POST /api/v1/subscriptions/checkout
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCheckout([FromBody] ChangeSubscriptionRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<SubscriptionResponse> { Error = "Unauthorized" });

        var result = await _billingService.CreateCheckoutSessionAsync(userId.Value, request.NewTier);
        
        if (!result.Success)
        {
            return BadRequest(new ApiResponse<SubscriptionResponse>
            {
                Success = false,
                Error = result.Message
            });
        }

        return Ok(new ApiResponse<SubscriptionResponse> { Success = true, Data = result });
    }

    /// <summary>
    /// POST /api/v1/subscriptions/portal
    /// </summary>
    [HttpPost("portal")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCustomerPortal()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<SubscriptionResponse> { Error = "Unauthorized" });

        var result = await _billingService.CreateCustomerPortalSessionAsync(userId.Value);
        
        if (!result.Success)
        {
            return BadRequest(new ApiResponse<SubscriptionResponse>
            {
                Success = false,
                Error = result.Message
            });
        }

        return Ok(new ApiResponse<SubscriptionResponse> { Success = true, Data = result });
    }

    /// <summary>
    /// DELETE /api/v1/subscriptions
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelSubscription()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new ApiResponse<SubscriptionResponse> { Error = "Unauthorized" });

        var result = await _billingService.CancelSubscriptionAsync(userId.Value);
        
        if (!result.Success)
        {
            return BadRequest(new ApiResponse<SubscriptionResponse>
            {
                Success = false,
                Error = result.Message
            });
        }

        return Ok(new ApiResponse<SubscriptionResponse> { Success = true, Data = result });
    }

    private Guid? GetUserId()
    {
        var userId = HttpContext.Items["UserId"] as string;
        return userId != null ? Guid.Parse(userId) : null;
    }
}