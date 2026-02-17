using System.ComponentModel.DataAnnotations;

namespace FleetFuel.Api.Models;

/// <summary>
/// Request DTO for changing password.
/// </summary>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for session information.
/// </summary>
public class SessionInfo
{
    public string SessionId { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public bool IsCurrent { get; set; }
}

/// <summary>
/// Response DTO for security info.
/// </summary>
public class SecurityInfoResponse
{
    public DateTime? LastPasswordChange { get; set; }
    public int ActiveSessionCount { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public List<SessionInfo> Sessions { get; set; } = new();
}

/// <summary>
/// Generic success response.
/// </summary>
public class SuccessResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}