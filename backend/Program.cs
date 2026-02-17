using FleetFuel.Api.Middleware;
using FleetFuel.Api.Repositories;
using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting FleetFuel API...");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHttpContextAccessor();

    // Configure SQLite database
    builder.Services.AddDbContext<FleetFuelDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DatabaseConnectionString")));

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            var frontendUrl = builder.Configuration["FRONTEND_URL"] 
                ?? builder.Configuration["Frontend:Url"] 
                ?? "http://localhost:3000";
            policy.WithOrigins(frontendUrl)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // Register repositories
    builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
    builder.Services.AddScoped<ITripRepository, TripRepository>();
    builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();

    // Register services
    builder.Services.AddScoped<IVehicleService, VehicleService>();
    builder.Services.AddScoped<ITripService, TripService>();
    builder.Services.AddScoped<IReceiptService, ReceiptService>();
    builder.Services.AddScoped<ISummaryService, SummaryService>();
    builder.Services.AddScoped<IYearLockService, YearLockService>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
    builder.Services.AddScoped<IBackupService, BackupService>();
    builder.Services.AddScoped<IMonitoringService, MonitoringService>();
    builder.Services.AddScoped<ISyncService, SyncService>();
    builder.Services.AddScoped<IDeploymentMonitor, DeploymentMonitor>();
    builder.Services.AddScoped<IUserService, UserService>();

    // Add HttpClient for external service calls
    builder.Services.AddHttpClient<IDeploymentMonitor, DeploymentMonitor>();

    // Add Supabase JWT authentication (TASK-16)
    // builder.Services.AddAuthentication(...).AddJwtBearer(...);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors("AllowFrontend");
    
    // Serve uploaded files
    app.UseStaticFiles();

    // Supabase JWT token validation (TASK-16)
    app.UseSupabaseJwtValidation();
    
    app.UseAuthorization();
    app.MapControllers();

    // Health check endpoints
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

    // Full system health check (includes deployment monitoring)
    app.MapGet("/health/full", async (IDeploymentMonitor monitor) => 
    {
        var report = await monitor.GetFullHealthReportAsync();
        return Results.Ok(report);
    });

    Log.Information("FleetFuel API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FleetFuel API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}