using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace FleetFuel.Api;

/// <summary>
/// Structured logging configuration for production observability.
/// </summary>
public static class LoggingConfiguration
{
    public static LoggerConfiguration ConfigureLogging(this LoggerConfiguration config, bool isDevelopment)
    {
        var environment = isDevelopment ? "Development" : "Production";

        config
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: isDevelopment ? AnsiConsoleTheme.Code : AnsiConsoleTheme.Literate
            );

        // In production, also write to file
        if (!isDevelopment)
        {
            config.WriteTo.File(
                path: "logs/fleetfuel-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
        }

        return config;
    }
}

/// <summary>
/// Request logging configuration for API observability.
/// </summary>
public static class RequestLoggingConfiguration
{
    public static void ConfigureRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                diagnosticContext.Set("ResponseStatusCode", httpContext.Response.StatusCode);

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
                }
            };

            options.GetLevel = (ctx, _, ex) =>
            {
                // Log at Warning level for 4xx errors, Error level for 5xx
                if (ex != null) return LogEventLevel.Error;
                if (ctx.Response.StatusCode >= 500) return LogEventLevel.Error;
                if (ctx.Response.StatusCode >= 400) return LogEventLevel.Warning;
                return LogEventLevel.Information;
            };
        });
    }
}
