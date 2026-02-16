using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using FleetFuel.Data;
using Serilog;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service for application monitoring, metrics, and health checks.
/// </summary>
public interface IMonitoringService
{
    Task<HealthCheckResponse> GetDetailedHealthCheckAsync();
    Task<AppMetrics> GetMetricsAsync();
    Task RecordUserActivityAsync(Guid userId);
    Task RecordTripCreatedAsync();
    Task RecordReceiptCreatedAsync();
    Task RecordExportAsync();
}

public class MonitoringService : IMonitoringService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<MonitoringService> _logger;
    private readonly ActivitySource _activitySource;
    private static DateTime _startTime = DateTime.UtcNow;
    private static int _restartCount = 0;
    private static int _totalTripsToday = 0;
    private static int _totalReceiptsToday = 0;
    private static int _totalExportsThisWeek = 0;
    private static DateTime _lastResetDate = DateTime.UtcNow.Date;

    public MonitoringService(
        FleetFuelDbContext context,
        ILogger<MonitoringService> logger)
    {
        _context = context;
        _logger = logger;
        _activitySource = new ActivitySource("FleetFuel.Api");
        
        // Reset daily counters
        ResetDailyCountersIfNeeded();
    }

    private void ResetDailyCountersIfNeeded()
    {
        if (_lastResetDate.Date != DateTime.UtcNow.Date)
        {
            _totalTripsToday = 0;
            _totalReceiptsToday = 0;
            _lastResetDate = DateTime.UtcNow.Date;
        }
    }

    public async Task<HealthCheckResponse> GetDetailedHealthCheckAsync()
    {
        var response = new HealthCheckResponse
        {
            Timestamp = DateTime.UtcNow,
            Uptime = new UptimeInfo
            {
                StartTime = _startTime,
                TotalUptimeHours = (DateTime.UtcNow - _startTime).TotalHours,
                RestartCount = _restartCount,
                LastRestart = _startTime
            }
        };

        // Check database
        var dbStopwatch = Stopwatch.StartNew();
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            dbStopwatch.Stop();
            response.Database = new ComponentHealth
            {
                Status = canConnect ? "healthy" : "degraded",
                ResponseTimeMs = dbStopwatch.ElapsedMilliseconds,
                LastChecked = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            dbStopwatch.Stop();
            response.Database = new ComponentHealth
            {
                Status = "unhealthy",
                ResponseTimeMs = dbStopwatch.ElapsedMilliseconds,
                ErrorMessage = ex.Message,
                LastChecked = DateTime.UtcNow
            };
        }

        // Storage health (placeholder - in production check Supabase storage)
        response.Storage = new ComponentHealth
        {
            Status = "healthy",
            ResponseTimeMs = 5,
            LastChecked = DateTime.UtcNow
        };

        // External services (Supabase)
        response.ExternalServices = new ComponentHealth
        {
            Status = "healthy",
            ResponseTimeMs = 50,
            LastChecked = DateTime.UtcNow
        };

        // Set overall status
        response.Status = response.Database.Status == "healthy" ? "healthy" : "degraded";

        return response;
    }

    public async Task<AppMetrics> GetMetricsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        try
        {
            var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
            var totalVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted);
            
            var activeUsersToday = await _context.Users
                .CountAsync(u => !u.IsDeleted); // In production, track last login

            var tripsThisWeek = await _context.Trips
                .CountAsync(t => t.CreatedAt >= weekAgo && !t.IsDeleted);
            
            var receiptsThisWeek = await _context.Receipts
                .CountAsync(r => r.CreatedAt >= weekAgo && !r.IsDeleted);

            var metrics = new AppMetrics
            {
                TotalUsers = totalUsers,
                ActiveUsersToday = activeUsersToday,
                ActiveUsersThisWeek = activeUsersToday,
                TotalVehicles = totalVehicles,
                TotalTripsToday = _totalTripsToday,
                TotalTripsThisWeek = tripsThisWeek,
                TotalReceiptsToday = _totalReceiptsToday,
                TotalExportsThisWeek = _totalExportsThisWeek,
                AverageTripsPerUserPerWeek = totalUsers > 0 ? (double)tripsThisWeek / totalUsers : 0,
                AverageReceiptsPerVehicle = totalVehicles > 0 ? (double)_totalReceiptsToday / totalVehicles : 0,
                LastUpdated = DateTime.UtcNow
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate metrics");
            return new AppMetrics { LastUpdated = DateTime.UtcNow };
        }
    }

    public Task RecordUserActivityAsync(Guid userId)
    {
        // In production, update user's last activity timestamp
        _logger.LogDebug("User activity recorded for {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task RecordTripCreatedAsync()
    {
        _totalTripsToday++;
        _logger.LogDebug("Trip created - daily count: {Count}", _totalTripsToday);
        return Task.CompletedTask;
    }

    public Task RecordReceiptCreatedAsync()
    {
        _totalReceiptsToday++;
        _logger.LogDebug("Receipt created - daily count: {Count}", _totalReceiptsToday);
        return Task.CompletedTask;
    }

    public Task RecordExportAsync()
    {
        _totalExportsThisWeek++;
        _logger.LogDebug("Export recorded - weekly count: {Count}", _totalExportsThisWeek);
        return Task.CompletedTask;
    }

    public static void IncrementRestartCount()
    {
        _restartCount++;
    }
}