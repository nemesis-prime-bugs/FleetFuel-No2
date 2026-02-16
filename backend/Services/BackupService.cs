using Microsoft.EntityFrameworkCore;
using FleetFuel.Data;
using Serilog;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service implementation for backup management.
/// Note: Supabase handles automatic daily backups.
/// This service provides status monitoring and reporting.
/// </summary>
public class BackupService : IBackupService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<BackupService> _logger;
    private readonly IConfiguration _configuration;

    public BackupService(
        FleetFuelDbContext context, 
        ILogger<BackupService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<BackupStatus> GetBackupStatusAsync()
    {
        var status = new BackupStatus
        {
            IsHealthy = true,
            Provider = "Supabase",
            BackupRetentionDays = 30,
            LastBackupTime = await GetLastBackupTimeAsync(),
            NextScheduledBackup = DateTime.UtcNow.AddHours(24)
        };

        // Check if we have recent backup (within 24 hours)
        if (status.LastBackupTime.HasValue)
        {
            var hoursSinceBackup = (DateTime.UtcNow - status.LastBackupTime.Value).TotalHours;
            if (hoursSinceBackup > 24)
            {
                status.IsHealthy = false;
                status.RecentBackupErrors.Add("Last backup was more than 24 hours ago");
            }
        }
        else
        {
            // No backup history - this is expected for new installations
            status.IsHealthy = true;
            status.RecentBackupErrors.Add("No backup history yet - first backup pending");
        }

        return status;
    }

    public async Task<bool> TriggerManualBackupAsync()
    {
        try
        {
            // In a real implementation, this would trigger Supabase pg_dump
            // For now, we log the request and return true
            
            _logger.LogInformation("Manual backup triggered at {Time}", DateTime.UtcNow);
            
            // Record the backup attempt
            var backupRecord = new BackupRecord
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Type = "Manual",
                SizeBytes = 0,
                Success = true,
                RetentionDays = 30,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger manual backup");
            return false;
        }
    }

    public async Task<DateTime?> GetLastBackupTimeAsync()
    {
        // In production, query Supabase backup metadata
        // For now, return null to indicate no recorded backup
        return null;
    }

    public async Task<IEnumerable<BackupRecord>> GetBackupHistoryAsync(int limit = 10)
    {
        // In production, this would query backup metadata from Supabase
        // Return mock data for development
        var mockRecords = new List<BackupRecord>
        {
            new BackupRecord
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow.AddHours(-12),
                Type = "Incremental",
                SizeBytes = 1024 * 1024 * 5, // 5MB
                Success = true,
                RetentionDays = 30,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            },
            new BackupRecord
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow.AddDays(-1),
                Type = "Full",
                SizeBytes = 1024 * 1024 * 50, // 50MB
                Success = true,
                RetentionDays = 30,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            }
        };

        return mockRecords.Take(limit);
    }
}