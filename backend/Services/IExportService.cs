using FleetFuel.Api.Models;

namespace FleetFuel.Api.Services;

/// <summary>
/// Interface for data export operations.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Generate data export for user.
    /// </summary>
    Task<ExportStatusResponse> GenerateExportAsync(Guid userId, ExportRequest request);

    /// <summary>
    /// Get export file for download.
    /// </summary>
    Task<byte[]?> GetExportFileAsync(Guid userId, Guid exportId);

    /// <summary>
    /// List user's export history.
    /// </summary>
    Task<List<ExportMetadata>> ListExportsAsync(Guid userId);

    /// <summary>
    /// Delete an export file.
    /// </summary>
    Task<bool> DeleteExportAsync(Guid userId, Guid exportId);
}