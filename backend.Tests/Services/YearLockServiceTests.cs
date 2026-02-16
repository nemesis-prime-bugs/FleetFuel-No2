using FleetFuel.Api.Services;
using FleetFuel.Data;
using FleetFuel.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FleetFuel.Api.Tests.Services;

/// <summary>
/// Unit tests for YearLockService following AAA pattern.
/// </summary>
public class YearLockServiceTests
{
    private readonly FleetFuelDbContext _context;
    private readonly YearLockService _service;
    private readonly Mock<ILogger<YearLockService>> _loggerMock;

    public YearLockServiceTests()
    {
        // Arrange - Setup test database
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new FleetFuelDbContext(options);
        _loggerMock = new Mock<ILogger<YearLockService>>();
        _service = new YearLockService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task LockYearAsync_WhenNotLocked_LocksSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;
        var reason = "Tax filing complete";

        // Act
        var result = await _service.LockYearAsync(userId, year, reason);

        // Assert
        Assert.True(result);
        Assert.True(await _service.IsYearLockedAsync(userId, year));
    }

    [Fact]
    public async Task LockYearAsync_WhenAlreadyLocked_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;
        
        await _service.LockYearAsync(userId, year, "First lock");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.LockYearAsync(userId, year, "Second lock"));
    }

    [Fact]
    public async Task UnlockYearAsync_WhenLocked_UnlocksSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid().ToString();
        var year = 2024;
        var unlockReason = "Correction needed";
        
        await _service.LockYearAsync(userId, year, "Initial lock");

        // Act
        var result = await _service.UnlockYearAsync(userId, year, adminUserId, unlockReason);

        // Assert
        Assert.True(result);
        Assert.False(await _service.IsYearLockedAsync(userId, year));
    }

    [Fact]
    public async Task UnlockYearAsync_WhenNotLocked_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.UnlockYearAsync(userId, year, Guid.NewGuid().ToString(), "Unlock"));
    }

    [Fact]
    public async Task IsYearLockedAsync_WhenNoSummary_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;

        // Act
        var result = await _service.IsYearLockedAsync(userId, year);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetOrCreateYearSummaryAsync_WhenNotExists_CreatesNew()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;

        // Act
        var summary = await _service.GetOrCreateYearSummaryAsync(userId, year);

        // Assert
        Assert.NotNull(summary);
        Assert.Equal(userId, summary.UserId);
        Assert.Equal(year, summary.Year);
        Assert.False(summary.IsLocked);
    }

    [Fact]
    public async Task GetOrCreateYearSummaryAsync_WhenExists_ReturnsExisting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var year = 2024;
        
        var firstSummary = await _service.GetOrCreateYearSummaryAsync(userId, year);

        // Act
        var secondSummary = await _service.GetOrCreateYearSummaryAsync(userId, year);

        // Assert
        Assert.Equal(firstSummary.Id, secondSummary.Id);
    }
}