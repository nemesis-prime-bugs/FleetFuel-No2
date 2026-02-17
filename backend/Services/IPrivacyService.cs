using FleetFuel.Api.Models;

namespace FleetFuel.Api.Services;

/// <summary>
/// Interface for privacy and data management operations.
/// </summary>
public interface IPrivacyService
{
    /// <summary>
    /// Get user's privacy settings.
    /// </summary>
    Task<PrivacySettings?> GetPrivacySettingsAsync(Guid userId);

    /// <summary>
    /// Update user's privacy settings.
    /// </summary>
    Task<PrivacySettings?> UpdatePrivacySettingsAsync(Guid userId, UpdatePrivacyRequest request);

    /// <summary>
    /// Request data export (already exists in IExportService, but adding for privacy flow).
    /// </summary>
    Task<DataExportStatus> RequestDataExportAsync(Guid userId);

    /// <summary>
    /// Request account deletion (right to be forgotten).
    /// </summary>
    Task<DeleteAccountResponse> RequestAccountDeletionAsync(Guid userId, DeleteAccountRequest request);

    /// <summary>
    /// Cancel pending account deletion.
    /// </summary>
    Task<bool> CancelAccountDeletionAsync(Guid userId);

    /// <summary>
    /// Anonymize user data (for GDPR compliance).
    /// </summary>
    Task<bool> AnonymizeDataAsync(Guid userId);
}