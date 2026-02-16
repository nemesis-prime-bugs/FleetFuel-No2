using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service interface for year lock operations.
/// </summary>
public interface IYearLockService
{
    Task<YearSummary> GetOrCreateYearSummaryAsync(Guid userId, int year);
    Task<bool> LockYearAsync(Guid userId, int year, string reason);
    Task<bool> UnlockYearAsync(Guid userId, int year, string adminUserId, string reason);
    Task<bool> IsYearLockedAsync(Guid userId, int year);
}

/// <summary>
/// Service implementation for year lock operations.
/// </summary>
public class YearLockService : IYearLockService
{
    private readonly FleetFuelDbContext _context;

    public YearLockService(FleetFuelDbContext context)
    {
        _context = context;
    }

    public async Task<YearSummary> GetOrCreateYearSummaryAsync(Guid userId, int year)
    {
        var summary = await _context.YearSummaries
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Year == year);

        if (summary == null)
        {
            summary = new YearSummary
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Year = year,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            _context.YearSummaries.Add(summary);
            await _context.SaveChangesAsync();
        }

        return summary;
    }

    public async Task<bool> LockYearAsync(Guid userId, int year, string reason)
    {
        var summary = await GetOrCreateYearSummaryAsync(userId, year);

        if (summary.IsLocked)
        {
            throw new InvalidOperationException($"Year {year} is already locked");
        }

        summary.IsLocked = true;
        summary.LockedAt = DateTime.UtcNow;
        summary.LockedByUserId = userId;
        summary.LockReason = reason;
        summary.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnlockYearAsync(Guid userId, int year, string adminUserId, string reason)
    {
        var summary = await GetOrCreateYearSummaryAsync(userId, year);

        if (!summary.IsLocked)
        {
            throw new InvalidOperationException($"Year {year} is not locked");
        }

        // Log the unlock action for audit purposes
        // In production, verify adminUserId has admin role

        summary.IsLocked = false;
        summary.LockedAt = null;
        summary.LockedByUserId = Guid.Parse(adminUserId);
        summary.LockReason = $"Unlocked: {reason}";
        summary.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsYearLockedAsync(Guid userId, int year)
    {
        var summary = await _context.YearSummaries
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Year == year);
        
        return summary?.IsLocked ?? false;
    }
}
