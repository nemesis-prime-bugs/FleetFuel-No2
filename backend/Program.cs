using FleetFuel.Api.Middleware;
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

    // Configure SQLite database
    builder.Services.AddDbContext<FleetFuelDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DatabaseConnectionString")));

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(
                builder.Configuration["Frontend:Url"] ?? "http://localhost:3000",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });

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
    
    // Supabase JWT token validation (TASK-16)
    app.UseSupabaseJwtValidation();
    
    app.UseAuthorization();
    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

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