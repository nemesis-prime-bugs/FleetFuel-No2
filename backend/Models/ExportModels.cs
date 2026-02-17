using System.ComponentModel.DataAnnotations;

namespace FleetFuel.Api.Models;

/// <summary>
/// Export format options.
/// </summary>
public enum ExportFormat
{
    Json,
    Csv
}

/// <summary>
/// Data type options for export.
/// </summary>
[Flags]
public enum ExportDataType
{
    None = 0,
    Vehicles = 1 << 0,
    Trips = 1 << 1,
    Receipts = 1 << 2,
    Profile = 1 << 3,
    All = Vehicles | Trips | Receipts | Profile
}

/// <summary>
/// Request DTO for data export.
/// </summary>
public class ExportRequest
{
    [Required]
    public ExportFormat Format { get; set; } = ExportFormat.Json;

    [Required]
    public ExportDataType DataTypes { get; set; } = ExportDataType.All;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Response DTO for export status.
/// </summary>
public class ExportStatusResponse
{
    public bool Success { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Message { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Export metadata for file info.
/// </summary>
public class ExportMetadata
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Format { get; set; } = string.Empty;
    public string DataTypes { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string FilePath { get; set; } = string.Empty;
}