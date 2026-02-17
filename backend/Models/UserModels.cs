using System.ComponentModel.DataAnnotations;

namespace FleetFuel.Api.Models;

/// <summary>
/// Response DTO for user profile data.
/// </summary>
public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EmailConfirmedAt { get; set; }
}

/// <summary>
/// Request DTO for updating user profile.
/// </summary>
public class UpdateProfileRequest
{
    [MaxLength(100, ErrorMessage = "Display name must be at most 100 characters")]
    public string? DisplayName { get; set; }

    [MaxLength(500, ErrorMessage = "Bio must be at most 500 characters")]
    public string? Bio { get; set; }
}

/// <summary>
/// Request DTO for uploading avatar.
/// </summary>
public class UploadAvatarRequest
{
    [Required(ErrorMessage = "Avatar data is required")]
    public string AvatarBase64 { get; set; } = string.Empty;

    [Required(ErrorMessage = "File extension is required")]
    public string FileExtension { get; set; } = string.Empty; // .jpg, .png, .gif
}

/// <summary>
/// User preferences for localization and display.
/// </summary>
public class UserPreferences
{
    // Display preferences
    public string Currency { get; set; } = "USD";
    public string DistanceUnit { get; set; } = "km";
    public string VolumeUnit { get; set; } = "L";
    public string FuelEfficiencyUnit { get; set; } = "L/100km";
    public string TemperatureUnit { get; set; } = "C";
    public string DateFormat { get; set; } = "DD/MM/YYYY";
    public string Theme { get; set; } = "system";
    public string Timezone { get; set; } = "UTC";
    
    // Notification preferences
    public bool ServiceReminders { get; set; } = true;
    public bool TripReports { get; set; } = false;
    public bool FuelPriceAlerts { get; set; } = false;
    public bool ProductUpdates { get; set; } = false;
    public bool SecurityAlerts { get; set; } = true;
    public string EmailFrequency { get; set; } = "realtime"; // realtime, daily, weekly, never
}

/// <summary>
/// Request DTO for updating user preferences.
/// </summary>
public class UpdatePreferencesRequest
{
    public UserPreferences? Preferences { get; set; }
}