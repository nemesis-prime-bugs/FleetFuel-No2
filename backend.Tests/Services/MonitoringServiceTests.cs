using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FleetFuel.Api.Tests.Services;

/// <summary>
/// Unit tests for MonitoringService following AAA pattern.
/// </summary>
public class MonitoringServiceTests
{
    private readonly FleetFuelDbContext _context;
    private readonly MonitoringService _service;
    private readonly Mock<ILogger<MonitoringService>> _loggerMock;

    public MonitoringServiceTests()
    {
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"MonitorTestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new FleetFuelDbContext(options);
        _loggerMock = new Mock<ILogger<MonitoringService>>();
        _service = new MonitoringService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetDetailedHealthCheckAsync_WhenDbHealthy_ReturnsHealthy()
    {
        // Act
        var response = await _service.GetDetailedHealthCheckAsync();

        // Assert
        Assert.Equal("healthy", response.Status);
        Assert.Equal("healthy", response.Database.Status);
        Assert.True(response.Database.ResponseTimeMs >= 0);
    }

    [Fact]
    public async Task GetDetailedHealthCheckAsync_ReturnsTimestamp()
    {
        // Act
        var response = await _service.GetDetailedHealthCheckAsync();

        // Assert
        Assert.True(response.Timestamp <= DateTime.UtcNow);
        Assert.True(response.Timestamp > DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public async Task GetDetailedHealthCheckAsync_IncludesUptime()
    {
        // Act
        var response = await _service.GetDetailedHealthCheckAsync();

        // Assert
        Assert.NotNull(response.Uptime);
        Assert.NotNull(response.Uptime.StartTime);
        Assert.True(response.Uptime.TotalUptimeHours >= 0);
    }

    [Fact]
    public async Task GetMetricsAsync_WhenNoData_ReturnsZeroCounts()
    {
        // Act
        var metrics = await _service.GetMetricsAsync();

        // Assert
        Assert.Equal(0, metrics.TotalUsers);
        Assert.Equal(0, metrics.TotalVehicles);
        Assert.Equal(0, metrics.TotalTripsToday);
        Assert.Equal(0, metrics.TotalReceiptsToday);
    }

    [Fact]
    public async Task GetMetricsAsync_WithData_ReturnsCorrectCounts()
    {
        // Arrange - Add test data
        var user = new Data.User { Id = Guid.NewGuid(), Email = "test@test.com", IsDeleted = false };
        _context.Users.Add(user);
        
        var vehicle = new Data.Vehicle { Id = Guid.NewGuid(), UserId = user.Id, LicensePlate = "TEST-123", IsDeleted = false };
        _context.Vehicles.Add(vehicle);
        
        await _context.SaveChangesAsync();

        // Act
        var metrics = await _service.GetMetricsAsync();

        // Assert
        Assert.Equal(1, metrics.TotalUsers);
        Assert.Equal(1, metrics.TotalVehicles);
    }

    [Fact]
    public async Task RecordTripCreatedAsync_DoesNotThrow()
    {
        // Act & Assert - Verify the method doesn't throw
        await _service.RecordTripCreatedAsync();
        await _service.RecordTripCreatedAsync();
    }

    [Fact]
    public async Task RecordReceiptCreatedAsync_DoesNotThrow()
    {
        // Act & Assert - Verify the method doesn't throw
        await _service.RecordReceiptCreatedAsync();
    }

    [Fact]
    public async Task RecordExportAsync_DoesNotThrow()
    {
        // Act & Assert - Verify the method doesn't throw
        await _service.RecordExportAsync();
    }

    [Fact]
    public async Task RecordUserActivityAsync_DoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await _service.RecordUserActivityAsync(userId);
    }

    [Fact]
    public async Task GetDetailedHealthCheckAsync_IncludesAllComponents()
    {
        // Act
        var response = await _service.GetDetailedHealthCheckAsync();

        // Assert
        Assert.NotNull(response.Database);
        Assert.NotNull(response.Storage);
        Assert.NotNull(response.ExternalServices);
        Assert.NotNull(response.Uptime);
        Assert.NotNull(response.Metrics);
    }
}