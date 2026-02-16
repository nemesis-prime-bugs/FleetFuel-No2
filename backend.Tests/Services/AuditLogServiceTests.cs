using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FleetFuel.Api.Tests.Services;

/// <summary>
/// Unit tests for AuditLogService following AAA pattern.
/// </summary>
public class AuditLogServiceTests
{
    private readonly FleetFuelDbContext _context;
    private readonly AuditLogService _service;
    private readonly Mock<ILogger<AuditLogService>> _loggerMock;

    public AuditLogServiceTests()
    {
        // Arrange - Setup test database
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuditTestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new FleetFuelDbContext(options);
        _loggerMock = new Mock<ILogger<AuditLogService>>();
        _service = new AuditLogService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task LogAsync_WithValidData_CreatesAuditLog()
    {
        // Arrange
        var entityName = "Vehicle";
        var entityId = Guid.NewGuid();
        var actionType = "Create";
        var userId = Guid.NewGuid();
        var oldValues = new { Name = "Old" };
        var newValues = new { Name = "New" };
        var reason = "Test creation";

        // Act
        await _service.LogAsync(entityName, entityId, actionType, userId, oldValues, newValues, reason);

        // Assert
        var logs = await _context.AuditLogs.ToListAsync();
        Assert.Single(logs);
        Assert.Equal(entityName, logs[0].EntityName);
        Assert.Equal(actionType, logs[0].ActionType);
        Assert.Equal(userId, logs[0].UserId);
    }

    [Fact]
    public async Task LogAsync_WithJsonStrings_StoresCorrectly()
    {
        // Arrange
        var entityName = "Trip";
        var entityId = Guid.NewGuid();
        var actionType = "Update";
        var userId = Guid.NewGuid();
        var oldJson = "{\"km\":100}";
        var newJson = "{\"km\":200}";

        // Act
        await _service.LogAsync(entityName, entityId, actionType, userId, oldJson, newJson, null);

        // Assert
        var log = await _context.AuditLogs.FirstAsync();
        Assert.Equal(oldJson, log.OldValues);
        Assert.Equal(newJson, log.NewValues);
    }

    [Fact]
    public async Task LogAsync_WhenDbFails_DoesNotThrow()
    {
        // Arrange
        var entityName = "Vehicle";
        var entityId = Guid.NewGuid();
        var actionType = "Delete";
        var userId = Guid.NewGuid();

        // Use a disposed context to simulate failure
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"FailDb_{Guid.NewGuid()}")
            .Options;
        
        using var failContext = new FleetFuelDbContext(options);
        await failContext.SaveChangesAsync(); // Create database
        failContext.Dispose();

        var failService = new AuditLogService(failContext, _loggerMock.Object);

        // Act & Assert - Should not throw
        await _service.LogAsync(entityName, entityId, actionType, userId, null, null, "Test");
    }

    [Fact]
    public async Task GetByEntityAsync_ReturnsCorrectLogs()
    {
        // Arrange
        var entityName = "Vehicle";
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _service.LogAsync(entityName, entityId, "Create", userId, null, null);
        await _service.LogAsync(entityName, entityId, "Update", userId, null, null);
        await _service.LogAsync(entityName, Guid.NewGuid(), "Create", userId, null, null);

        // Act
        var logs = await _service.GetByEntityAsync(entityName, entityId);

        // Assert
        Assert.Equal(2, logs.Count());
        Assert.All(logs, log => Assert.Equal(entityName, log.EntityName));
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsCorrectLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await _service.LogAsync("Vehicle", Guid.NewGuid(), "Create", userId, null, null);
        await _service.LogAsync("Trip", Guid.NewGuid(), "Create", userId, null, null);
        await _service.LogAsync("Vehicle", Guid.NewGuid(), "Create", otherUserId, null, null);

        // Act
        var logs = await _service.GetByUserAsync(userId, limit: 10);

        // Assert
        Assert.Equal(2, logs.Count());
        Assert.All(logs, log => Assert.Equal(userId, log.UserId));
    }

    [Fact]
    public async Task GetByUserAsync_WithLimit_RespectsLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();

        for (int i = 0; i < 5; i++)
        {
            await _service.LogAsync("Vehicle", Guid.NewGuid(), "Create", userId, null, null);
        }

        // Act
        var logs = await _service.GetByUserAsync(userId, limit: 3);

        // Assert
        Assert.Equal(3, logs.Count());
    }

    [Fact]
    public async Task LogAsync_SetsTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);
        var userId = Guid.NewGuid();

        // Act
        await _service.LogAsync("Vehicle", Guid.NewGuid(), "Create", userId, null, null);

        // Assert
        var log = await _context.AuditLogs.FirstAsync();
        Assert.True(log.Timestamp >= before);
        Assert.True(log.Timestamp <= DateTime.UtcNow);
    }
}