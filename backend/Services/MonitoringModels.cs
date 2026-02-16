namespace FleetFuel.Api.Services;

/// <summary>
/// Application metrics for monitoring and analytics.
/// </summary>
public class AppMetrics
{
    public int TotalUsers { get; set; }
    public int ActiveUsersToday { get; set; }
    public int ActiveUsersThisWeek { get; set; }
    public int TotalVehicles { get; set; }
    public int TotalTripsToday { get; set; }
    public int TotalTripsThisWeek { get; set; }
    public int TotalReceiptsToday { get; set; }
    public int TotalExportsThisWeek { get; set; }
    public double AverageTripsPerUserPerWeek { get; set; }
    public double AverageReceiptsPerVehicle { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Health check response with detailed status.
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ComponentHealth Database { get; set; } = null!;
    public ComponentHealth Storage { get; set; } = null!;
    public ComponentHealth ExternalServices { get; set; } = null!;
    public UptimeInfo Uptime { get; set; } = null!;
    public AppMetrics Metrics { get; set; } = null!;
}

/// <summary>
/// Health status of a single component.
/// </summary>
public class ComponentHealth
{
    public string Status { get; set; } = null!;
    public long ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LastChecked { get; set; }
}

/// <summary>
/// Application uptime information.
/// </summary>
public class UptimeInfo
{
    public DateTime StartTime { get; set; }
    public double TotalUptimeHours { get; set; }
    public int RestartCount { get; set; }
    public DateTime? LastRestart { get; set; }
}