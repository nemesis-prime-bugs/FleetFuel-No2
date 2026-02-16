using System.Text.Json;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service interface for audit logging operations.
/// </summary>
public interface IAuditLogService
{
    Task LogAsync(string entityName, Guid entityId, string actionType, Guid userId, object? oldValues, object? newValues, string? reason = null);
    Task LogAsync(string entityName, Guid entityId, string actionType, Guid userId, string? oldValuesJson, string? newValuesJson, string? reason = null);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId);
    Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId, int limit = 100);
}

/// <summary>
/// Service implementation for audit logging.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(FleetFuelDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string entityName, Guid entityId, string actionType, Guid userId, object? oldValues, object? newValues, string? reason = null)
    {
        var oldJson = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
        var newJson = newValues != null ? JsonSerializer.Serialize(newValues) : null;
        
        await LogAsync(entityName, entityId, actionType, userId, oldJson, newJson, reason);
    }

    public async Task LogAsync(string entityName, Guid entityId, string actionType, Guid userId, string? oldValuesJson, string? newValuesJson, string? reason = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                ActionType = actionType,
                OldValues = oldValuesJson,
                NewValues = newValuesJson,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Reason = reason
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit log created: {Entity}/{Action} for user {UserId}", 
                entityName, actionType, userId);
        }
        catch (Exception ex)
        {
            // Never let audit logging failures affect main operations
            _logger.LogError(ex, "Failed to create audit log for {Entity}/{Action}", entityName, actionType);
        }
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId, int limit = 100)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}
