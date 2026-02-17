using System.Text.Json;
using FleetFuel.Api.Models;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Services;

/// <summary>
/// Implementation of user profile and preferences management.
/// </summary>
public class UserService : IUserService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _avatarStoragePath;

    public UserService(
        FleetFuelDbContext context,
        ILogger<UserService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
        _avatarStoragePath = Path.Combine(environment.ContentRootPath, "wwwroot", "avatars");
        
        // Ensure avatar directory exists
        if (!Directory.Exists(_avatarStoragePath))
        {
            Directory.CreateDirectory(_avatarStoragePath);
        }
    }

    public async Task<UserProfileResponse?> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        return MapToProfileResponse(user);
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        // Update profile fields
        if (request.DisplayName != null)
        {
            user.DisplayName = request.DisplayName.Trim();
        }

        if (request.Bio != null)
        {
            user.Bio = request.Bio.Trim();
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("User profile updated: {UserId}", userId);

        return MapToProfileResponse(user);
    }

    public async Task<UserProfileResponse?> UploadAvatarAsync(Guid userId, UploadAvatarRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        try
        {
            // Validate file extension
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = request.FileExtension.ToLowerInvariant();
            if (!validExtensions.Contains(extension))
            {
                throw new ArgumentException($"Invalid file extension. Allowed: {string.Join(", ", validExtensions)}");
            }

            // Validate base64 data
            var avatarBytes = Convert.FromBase64String(request.AvatarBase64);
            const int maxFileSize = 2 * 1024 * 1024; // 2MB
            if (avatarBytes.Length > maxFileSize)
            {
                throw new ArgumentException("File size exceeds 2MB limit");
            }

            // Generate unique filename
            var filename = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            var filepath = Path.Combine(_avatarStoragePath, filename);

            // Save file
            await File.WriteAllBytesAsync(filepath, avatarBytes);

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var oldFilename = Path.GetFileName(user.AvatarUrl);
                var oldFilepath = Path.Combine(_avatarStoragePath, oldFilename);
                if (File.Exists(oldFilepath))
                {
                    File.Delete(oldFilepath);
                }
            }

            // Update user avatar URL
            user.AvatarUrl = $"/avatars/{filename}";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User avatar uploaded: {UserId}", userId);

            return MapToProfileResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload avatar for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfileResponse?> RemoveAvatarAsync(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        // Delete avatar file if exists
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var filename = Path.GetFileName(user.AvatarUrl);
            var filepath = Path.Combine(_avatarStoragePath, filename);
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        user.AvatarUrl = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User avatar removed: {UserId}", userId);

        return MapToProfileResponse(user);
    }

    public async Task<UserPreferences?> GetPreferencesAsync(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        if (string.IsNullOrEmpty(user.PreferencesJson))
        {
            return new UserPreferences();
        }

        try
        {
            return JsonSerializer.Deserialize<UserPreferences>(user.PreferencesJson);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize preferences for user: {UserId}", userId);
            return new UserPreferences();
        }
    }

    public async Task<UserPreferences?> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        var currentPreferences = string.IsNullOrEmpty(user.PreferencesJson)
            ? new UserPreferences()
            : JsonSerializer.Deserialize<UserPreferences>(user.PreferencesJson) ?? new UserPreferences();

        // Merge with new preferences
        if (request.Preferences != null)
        {
            currentPreferences.Currency = request.Preferences.Currency ?? currentPreferences.Currency;
            currentPreferences.DistanceUnit = request.Preferences.DistanceUnit ?? currentPreferences.DistanceUnit;
            currentPreferences.VolumeUnit = request.Preferences.VolumeUnit ?? currentPreferences.VolumeUnit;
            currentPreferences.FuelEfficiencyUnit = request.Preferences.FuelEfficiencyUnit ?? currentPreferences.FuelEfficiencyUnit;
            currentPreferences.TemperatureUnit = request.Preferences.TemperatureUnit ?? currentPreferences.TemperatureUnit;
            currentPreferences.DateFormat = request.Preferences.DateFormat ?? currentPreferences.DateFormat;
            currentPreferences.Theme = request.Preferences.Theme ?? currentPreferences.Theme;
            currentPreferences.Timezone = request.Preferences.Timezone ?? currentPreferences.Timezone;
        }

        user.PreferencesJson = JsonSerializer.Serialize(currentPreferences);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User preferences updated: {UserId}", userId);

        return currentPreferences;
    }

    private static UserProfileResponse MapToProfileResponse(User user)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }
}