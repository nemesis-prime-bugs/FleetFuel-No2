namespace FleetFuel.Api.Services;

/// <summary>
/// Interface for backup management operations.
/// </summary>
public interface IBackupService
{
    Task<BackupStatus> GetBackupStatusAsync();
    Task<bool> TriggerManualBackupAsync();
    Task<DateTime?> GetLastBackupTimeAsync();
    Task<IEnumerable<BackupRecord>> GetBackupHistoryAsync(int limit = 10);
}

/// <summary>
/// Represents the current backup status.
/// </summary>
public class BackupStatus
{
    public bool IsHealthy { get; set; }
    public DateTime? LastBackupTime { get; set; }
    public DateTime? NextScheduledBackup { get; set; }
    public int BackupRetentionDays { get; set; } = 30;
    public string Provider { get; set; } = "Supabase";
    public List<string> RecentBackupErrors { get; set; } = new();
}

/// <summary>
/// Represents a backup record.
/// </summary>
public class BackupRecord
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = null!; // Full, Incremental
    public long SizeBytes { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetentionDays { get; set; }
    public DateTime ExpiresAt { get; set; }
}