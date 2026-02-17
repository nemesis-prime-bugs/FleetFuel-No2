using FleetFuel.Api.Models;

namespace FleetFuel.Api.Services;

/// <summary>
/// Interface for account security operations.
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Change user password.
    /// </summary>
    Task<SuccessResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

    /// <summary>
    /// Get user's security information including sessions.
    /// </summary>
    Task<SecurityInfoResponse?> GetSecurityInfoAsync(Guid userId);

    /// <summary>
    /// Get all user sessions.
    /// </summary>
    Task<List<SessionInfo>> GetSessionsAsync(Guid userId);

    /// <summary>
    /// Revoke a specific session.
    /// </summary>
    Task<bool> RevokeSessionAsync(Guid userId, string sessionId);

    /// <summary>
    /// Revoke all sessions except the current one.
    /// </summary>
    Task<bool> RevokeAllSessionsAsync(Guid userId);

    /// <summary>
    /// Invalidate all refresh tokens for user (used after password change).
    /// </summary>
    Task InvalidateAllTokensAsync(Guid userId);
}