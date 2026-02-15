using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FleetFuel.Api.Middleware;

/// <summary>
/// Middleware to validate Supabase JWT tokens and extract user information.
/// </summary>
public class SupabaseJwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SupabaseJwtValidationMiddleware> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SupabaseJwtValidationMiddleware(
        RequestDelegate next,
        ILogger<SupabaseJwtValidationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader["Bearer ".Length..];

            try
            {
                var userId = await ValidateTokenAndGetUserId(token);
                if (userId != null)
                {
                    // Add user claims to context
                    context.Items["UserId"] = userId;
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId),
                            new Claim("supabase_token", token)
                        },
                        JwtBearerDefaults.AuthenticationScheme));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate Supabase token");
            }
        }

        await _next(context);
    }

    private async Task<string?> ValidateTokenAndGetUserId(string token)
    {
        try
        {
            // Fetch JWKS from Supabase
            var supabaseUrl = _configuration["Supabase:Url"];
            var jwksUrl = $"{supabaseUrl}/jwt/v1/verify";

            var response = await _httpClient.PostAsync(jwksUrl, new StringContent(
                JsonSerializer.Serialize(new { token }),
                Encoding.UTF8,
                "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                
                if (result.TryGetProperty("user_id", out var userId))
                {
                    return userId.GetString();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Supabase token");
            return null;
        }
    }
}

public static class SupabaseJwtValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseSupabaseJwtValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SupabaseJwtValidationMiddleware>();
    }
}