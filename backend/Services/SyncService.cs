using System.Text.Json;
using FleetFuel.Data;

namespace FleetFuel.Api.Services;

/// <summary>
/// Sync operation types for conflict tracking.
/// </summary>
public enum SyncOperationType
{
    Create = 1,
    Update = 2,
    Delete = 3
}

/// <summary>
/// Represents a pending sync operation.
/// </summary>
public class SyncOperation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EntityType { get; set; } = null!; // Vehicle, Trip, Receipt
    public Guid EntityId { get; set; }
    public SyncOperationType OperationType { get; set; }
    public string? Payload { get; set; } // JSON
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

/// <summary>
/// Conflict resolution result.
/// </summary>
public class ConflictResolution
{
    public bool ConflictDetected { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public DateTime ServerTimestamp { get; set; }
    public DateTime ClientTimestamp { get; set; }
    public string Resolution { get; set; } = null!; // ServerWins, ClientWins, Merge
    public string? MergedData { get; set; }
}

/// <summary>
/// Interface for sync operations.
/// </summary>
public interface ISyncService
{
    Task QueueSyncOperationAsync(SyncOperation operation);
    Task<IEnumerable<SyncOperation>> GetPendingOperationsAsync(Guid userId);
    Task<bool> ProcessPendingOperationsAsync(Guid userId);
    Task<ConflictResolution> DetectConflictAsync(string entityType, Guid entityId, DateTime clientTimestamp);
    Task<bool> ResolveConflictAsync(Guid operationId, string resolution);
}

/// <summary>
/// Service for managing multi-device sync and conflict resolution.
/// </summary>
public class SyncService : ISyncService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<SyncService> _logger;

    public SyncService(FleetFuelDbContext context, ILogger<SyncService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task QueueSyncOperationAsync(SyncOperation operation)
    {
        operation.Id = Guid.NewGuid();
        operation.CreatedAt = DateTime.UtcNow;
        operation.RetryCount = 0;

        _context.SyncOperations.Add(operation);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Queued sync operation: {EntityType}/{EntityId}", 
            operation.EntityType, operation.EntityId);
    }

    public async Task<IEnumerable<SyncOperation>> GetPendingOperationsAsync(Guid userId)
    {
        return await _context.SyncOperations
            .Where(o => o.UserId == userId && o.ProcessedAt == null)
            .OrderBy(o => o.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task<bool> ProcessPendingOperationsAsync(Guid userId)
    {
        var operations = await GetPendingOperationsAsync(userId);
        var processed = 0;

        foreach (var operation in operations)
        {
            try
            {
                // Process based on operation type
                switch (operation.OperationType)
                {
                    case SyncOperationType.Create:
                        // Handle create
                        break;
                    case SyncOperationType.Update:
                        // Handle update
                        break;
                    case SyncOperationType.Delete:
                        // Handle delete
                        break;
                }

                operation.ProcessedAt = DateTime.UtcNow;
                processed++;
            }
            catch (Exception ex)
            {
                operation.RetryCount++;
                _logger.LogError(ex, "Failed to process sync operation {Id}", operation.Id);
                
                if (operation.RetryCount >= 3)
                {
                    operation.ProcessedAt = DateTime.UtcNow;
                    _logger.LogWarning("Sync operation {Id} failed after 3 retries", operation.Id);
                }
            }
        }

        await _context.SaveChangesAsync();
        return processed > 0;
    }

    public async Task<ConflictResolution> DetectConflictAsync(string entityType, Guid entityId, DateTime clientTimestamp)
    {
        var resolution = new ConflictResolution
        {
            ConflictDetected = false,
            EntityType = entityType,
            EntityId = entityId,
            ClientTimestamp = clientTimestamp,
            ServerTimestamp = DateTime.UtcNow
        };

        // In production, query the entity and compare timestamps
        // For now, return no conflict
        resolution.Resolution = "NoConflict";
        return resolution;
    }

    public async Task<bool> ResolveConflictAsync(Guid operationId, string resolution)
    {
        var operation = await _context.SyncOperations
            .FindAsync(operationId);

        if (operation == null) return false;

        // Apply resolution
        operation.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}