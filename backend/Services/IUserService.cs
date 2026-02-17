using FleetFuel.Api.Models;

namespace FleetFuel.Api.Services;

/// <summary>
/// Interface for user profile and preferences management.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user profile by ID.
    /// </summary>
    Task<UserProfileResponse?> GetProfileAsync(Guid userId);

    /// <summary>
    /// Update user profile (display name, bio).
    /// </summary>
    Task<UserProfileResponse?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);

    /// <summary>
    /// Upload and set user avatar.
    /// </summary>
    Task<UserProfileResponse?> UploadAvatarAsync(Guid userId, UploadAvatarRequest request);

    /// <summary>
    /// Remove user avatar (revert to default).
    /// </summary>
    Task<UserProfileResponse?> RemoveAvatarAsync(Guid userId);

    /// <summary>
    /// Get user preferences.
    /// </summary>
    Task<UserPreferences?> GetPreferencesAsync(Guid userId);

    /// <summary>
    /// Update user preferences.
    /// </summary>
    Task<UserPreferences?> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request);
}