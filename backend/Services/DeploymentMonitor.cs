using System.Text.Json;
using Serilog;

namespace FleetFuel.Api.Services;

/// <summary>
/// Deployment status from external providers.
/// </summary>
public class DeploymentStatus
{
    public string Provider { get; set; } = null!;
    public string Status { get; set; } = null!; // building, success, failed
    public DateTime? LastCheck { get; set; }
    public DateTime? LastDeployment { get; set; }
    public string? Url { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Monitor for external deployments and build status.
/// </summary>
public interface IDeploymentMonitor
{
    Task<DeploymentStatus> GetRenderStatusAsync();
    Task<DeploymentStatus> GetVercelStatusAsync();
    Task<bool> IsBackendHealthyAsync();
    Task LogDeploymentEventAsync(string provider, string status, string? message);
}

public class DeploymentMonitor : IDeploymentMonitor
{
    private readonly HttpClient _http;
    private readonly ILogger<DeploymentMonitor> _logger;
    private const string RENDER_URL = "https://fleetfuel.onrender.com";
    private const string FRONTEND_URL = "https://frontend-iota-lac-88.vercel.app";

    public DeploymentMonitor(HttpClient http, ILogger<DeploymentMonitor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<DeploymentStatus> GetRenderStatusAsync()
    {
        var status = new DeploymentStatus
        {
            Provider = "Render",
            Url = RENDER_URL,
            LastCheck = DateTime.UtcNow
        };

        try
        {
            // Check health endpoint
            var healthResponse = await _http.GetAsync($"{RENDER_URL}/health");
            if (healthResponse.IsSuccessStatusCode)
            {
                status.Status = "healthy";
                status.LastDeployment = DateTime.UtcNow;
            }
            else
            {
                status.Status = "degraded";
                status.Errors.Add($"Health check returned: {(int)healthResponse.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            status.Status = "failed";
            status.Errors.Add(ex.Message);
            _logger.LogError(ex, "Render deployment check failed");
        }

        return status;
    }

    public async Task<DeploymentStatus> GetVercelStatusAsync()
    {
        var status = new DeploymentStatus
        {
            Provider = "Vercel",
            Url = FRONTEND_URL,
            LastCheck = DateTime.UtcNow
        };

        try
        {
            var response = await _http.GetAsync(FRONTEND_URL);
            if (response.IsSuccessStatusCode)
            {
                status.Status = "healthy";
                status.LastDeployment = DateTime.UtcNow;
            }
            else
            {
                status.Status = "degraded";
                status.Errors.Add($"Status code: {(int)response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            status.Status = "failed";
            status.Errors.Add(ex.Message);
            _logger.LogError(ex, "Vercel deployment check failed");
        }

        return status;
    }

    public async Task<bool> IsBackendHealthyAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{RENDER_URL}/health");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content.Contains("healthy");
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogDeploymentEventAsync(string provider, string status, string? message)
    {
        _logger.LogInformation("Deployment Event: {Provider} - {Status} - {Message}", 
            provider, status, message);
    }

    /// <summary>
    /// Full system health check - call this periodically
    /// </summary>
    public async Task<SystemHealthReport> GetFullHealthReportAsync()
    {
        var report = new SystemHealthReport
        {
            Timestamp = DateTime.UtcNow,
            RenderStatus = await GetRenderStatusAsync(),
            VercelStatus = await GetVercelStatusAsync()
        };

        report.AllHealthy = report.RenderStatus.Status == "healthy" 
            && report.VercelStatus.Status == "healthy";

        // Log health status
        if (report.AllHealthy)
        {
            _logger.LogInformation("System health check: ALL SYSTEMS HEALTHY");
        }
        else
        {
            _logger.LogWarning("System health check: ISSUES DETECTED");
            if (report.RenderStatus.Status != "healthy")
            {
                _logger.LogWarning("Render: {Status}", report.RenderStatus.Status);
            }
            if (report.VercelStatus.Status != "healthy")
            {
                _logger.LogWarning("Vercel: {Status}", report.VercelStatus.Status);
            }
        }

        return report;
    }
}

/// <summary>
/// Combined health report for all services.
/// </summary>
public class SystemHealthReport
{
    public DateTime Timestamp { get; set; }
    public DeploymentStatus RenderStatus { get; set; } = null!;
    public DeploymentStatus VercelStatus { get; set; } = null!;
    public bool AllHealthy { get; set; }
}