using System.ComponentModel.DataAnnotations;

namespace FleetFuel.Api.Models;

/// <summary>
/// Privacy settings for user data control.
/// </summary>
public class PrivacySettings
{
    public bool ShareAnalytics { get; set; } = false;
    public bool AllowPersonalizedAds { get; set; } = false;
    public bool EnableCookies { get; set; } = true;
    public bool AllowThirdPartySharing { get; set; } = false;
    public bool ShowProfilePublicly { get; set; } = false;
}

/// <summary>
/// Request DTO for updating privacy settings.
/// </summary>
public class UpdatePrivacyRequest
{
    public PrivacySettings? Settings { get; set; }
}

/// <summary>
/// Response DTO for data export status.
/// </summary>
public class DataExportStatus
{
    public bool Success { get; set; }
    public string? DownloadUrl { get; set; }
    public DateTime? AvailableUntil { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Request DTO for data deletion (right to be forgotten).
/// </summary>
public class DeleteAccountRequest
{
    [Required(ErrorMessage = "Confirmation is required")]
    public string Confirmation { get; set; } = string.Empty;

    public bool ExportDataFirst { get; set; } = true;
}

/// <summary>
/// Response for account deletion.
/// </summary>
public class DeleteAccountResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DateTime? DeletionScheduledAt { get; set; }
    public DateTime? DeletionCompletedAt { get; set; }
}