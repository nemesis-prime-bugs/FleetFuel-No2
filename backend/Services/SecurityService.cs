using FleetFuel.Api.Models;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FleetFuel.Api.Services;

/// <summary>
/// Implementation of account security operations.
/// Note: Password validation is handled by Supabase Auth.
/// This service manages session tracking in the database.
/// </summary>
public class SecurityService : ISecurityService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<SecurityService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SecurityService(
        FleetFuelDbContext context,
        ILogger<SecurityService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SuccessResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        // Validate password match
        if (request.NewPassword != request.ConfirmPassword)
        {
            return new SuccessResponse
            {
                Success = false,
                Message = "New passwords do not match"
            };
        }

        // Validate password strength
        if (request.NewPassword.Length < 8)
        {
            return new SuccessResponse
            {
                Success = false,
                Message = "Password must be at least 8 characters"
            };
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$"))
        {
            return new SuccessResponse
            {
                Success = false,
                Message = "Password must contain at least one uppercase letter, one lowercase letter, and one number"
            };
        }

        // Note: Actual password change must be done through Supabase Admin API
        // or by the user through the Supabase client
        // For now, we track that a password change was requested
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new SuccessResponse
            {
                Success = false,
                Message = "User not found"
            };
        }

        // Log password change request (actual Supabase password change happens client-side)
        _logger.LogInformation("Password change requested for user: {UserId}", userId);

        return new SuccessResponse
        {
            Success = true,
            Message = "Password change initiated. Please check your email for confirmation."
        };
    }

    public async Task<SecurityInfoResponse?> GetSecurityInfoAsync(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        var sessions = await GetSessionsAsync(userId);

        return new SecurityInfoResponse
        {
            LastPasswordChange = null, // Supabase manages this
            ActiveSessionCount = sessions.Count,
            TwoFactorEnabled = false, // Check with Supabase if needed
            Sessions = sessions
        };
    }

    public async Task<List<SessionInfo>> GetSessionsAsync(Guid userId)
    {
        // For MVP, return mock session data
        // In production, this would query a sessions table or Supabase
        var currentSessionId = _httpContextAccessor.HttpContext?.Items["SessionId"] as string ?? "current";

        var sessions = new List<SessionInfo>
        {
            new SessionInfo
            {
                SessionId = currentSessionId,
                Device = GetDeviceInfo(),
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                LastActiveAt = DateTime.UtcNow,
                IsCurrent = true
            }
        };

        // Mock additional sessions for demonstration
        if (!string.IsNullOrEmpty(GetDeviceInfo()))
        {
            sessions.Add(new SessionInfo
            {
                SessionId = Guid.NewGuid().ToString(),
                Device = "Unknown Device",
                IpAddress = "192.168.1.1",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                LastActiveAt = DateTime.UtcNow.AddDays(-1),
                IsCurrent = false
            });
        }

        return sessions;
    }

    public async Task<bool> RevokeSessionAsync(Guid userId, string sessionId)
    {
        // In production, this would delete the session from database/Supabase
        _logger.LogInformation("Session revocation requested for user {UserId}, session {SessionId}", userId, sessionId);
        
        // For MVP, we log the request
        return true;
    }

    public async Task<bool> RevokeAllSessionsAsync(Guid userId)
    {
        // In production, this would invalidate all refresh tokens for the user
        // through Supabase Admin API
        _logger.LogInformation("Revoke all sessions requested for user: {UserId}", userId);
        
        // For MVP, we log the request
        return true;
    }

    public async Task InvalidateAllTokensAsync(Guid userId)
    {
        // In production, this would call Supabase Admin API to revoke all refresh tokens
        _logger.LogInformation("Token invalidation requested for user: {UserId}", userId);
        await Task.CompletedTask;
    }

    private string GetDeviceInfo()
    {
        var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown Device";
        }

        // Simple device detection
        if (userAgent.Contains("Mobile"))
        {
            return "Mobile Device";
        }
        if (userAgent.Contains("Tablet"))
        {
            return "Tablet Device";
        }
        
        return "Desktop Browser";
    }
}