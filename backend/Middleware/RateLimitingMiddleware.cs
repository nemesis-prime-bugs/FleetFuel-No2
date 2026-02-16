using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FleetFuel.Api.Middleware;

/// <summary>
/// Rate limiting middleware to prevent brute force attacks.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;

    // In-memory storage for rate limit tracking
    // In production, use Redis or distributed cache
    private static readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();

    public RateLimitingMiddleware(RequestDelegate next, RateLimitOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetClientKey(context);

        // Clean up expired buckets periodically
        if (ShouldCleanup())
        {
            CleanupExpiredBuckets();
        }

        var bucket = _buckets.GetOrAdd(key, _ => new RateLimitBucket
        {
            RemainingRequests = _options.MaxRequests,
            ResetTime = DateTime.UtcNow.AddSeconds(_options.WindowSeconds)
        });

        // Check if bucket needs reset
        if (DateTime.UtcNow > bucket.ResetTime)
        {
            bucket.RemainingRequests = _options.MaxRequests;
            bucket.ResetTime = DateTime.UtcNow.AddSeconds(_options.WindowSeconds);
        }

        // Check rate limit
        if (bucket.RemainingRequests <= 0)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = _options.WindowSeconds.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = ((DateTimeOffset)bucket.ResetTime).ToUnixTimeSeconds().ToString();

            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        // Decrement remaining requests
        Interlocked.Decrement(ref bucket.RemainingRequests);

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = bucket.RemainingRequests.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = ((DateTimeOffset)bucket.ResetTime).ToUnixTimeSeconds().ToString();

        await _next(context);
    }

    private static string GetClientKey(HttpContext context)
    {
        // Use X-Forwarded-For for proxied requests, fallback to remote IP
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static bool ShouldCleanup()
    {
        // Cleanup 1% of the time
        return Random.Shared.Next(100) == 0;
    }

    private static void CleanupExpiredBuckets()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _buckets)
        {
            if (now > kvp.Value.ResetTime.AddMinutes(5))
            {
                _buckets.TryRemove(kvp.Key, out _);
            }
        }
    }
}

public class RateLimitBucket
{
    public int RemainingRequests;
    public DateTime ResetTime;
}

public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
}

public static class RateLimitingMiddlewareExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, Action<RateLimitOptions>? configure = null)
    {
        var options = new RateLimitOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        return services;
    }

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<RateLimitOptions>();
        return app.UseMiddleware<RateLimitingMiddleware>(options);
    }
}
