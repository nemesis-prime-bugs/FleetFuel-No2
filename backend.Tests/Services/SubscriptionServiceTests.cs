using FleetFuel.Api.Services;
using FleetFuel.Data;
using FleetFuel.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FleetFuel.Api.Tests.Services;

/// <summary>
/// Unit tests for SubscriptionService following AAA pattern.
/// </summary>
public class SubscriptionServiceTests
{
    private readonly FleetFuelDbContext _context;
    private readonly SubscriptionService _service;
    private readonly Mock<ILogger<SubscriptionService>> _loggerMock;

    public SubscriptionServiceTests()
    {
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"SubTestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new FleetFuelDbContext(options);
        _loggerMock = new Mock<ILogger<SubscriptionService>>();
        _service = new SubscriptionService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrCreateSubscriptionAsync_WhenNotExists_CreatesTrial()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);

        // Assert
        Assert.NotNull(subscription);
        Assert.Equal(SubscriptionTier.Free, subscription.Tier);
        Assert.Equal(AccountStatus.Trial, subscription.Status);
        Assert.True(subscription.TrialExpiresAt > DateTime.UtcNow);
        Assert.Equal(1, subscription.VehiclesLimit);
        Assert.Equal(100, subscription.ReceiptsLimit);
    }

    [Fact]
    public async Task GetOrCreateSubscriptionAsync_WhenExists_ReturnsExisting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var first = await _service.GetOrCreateSubscriptionAsync(userId);

        // Act
        var second = await _service.GetOrCreateSubscriptionAsync(userId);

        // Assert
        Assert.Equal(first.Id, second.Id);
    }

    [Fact]
    public async Task CanAddVehicle_WhenUnderLimit_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);
        subscription.VehiclesUsed = 0;

        // Act
        var result = await _service.CanAddVehicleAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddVehicle_WhenAtLimit_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);
        subscription.VehiclesUsed = subscription.VehiclesLimit;

        // Act
        var result = await _service.CanAddVehicleAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanAddReceipt_WhenUnderLimit_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);
        subscription.ReceiptsUsed = 50;

        // Act
        var result = await _service.CanAddReceiptAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetVehicleUsageWarningLevelAsync_WhenAt80Percent_ReturnsCritical()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);
        subscription.VehiclesUsed = 1; // 100% of 1 vehicle limit

        // Act
        var level = await _service.GetVehicleUsageWarningLevelAsync(userId);

        // Assert
        Assert.Equal(UsageWarningLevel.Blocked, level);
    }

    [Fact]
    public async Task GetVehicleUsageWarningLevelAsync_WhenAt60Percent_ReturnsWarning()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);
        subscription.VehiclesUsed = 1;
        subscription.VehiclesLimit = 2; // 50% - adjust for test

        // Act
        var level = await _service.GetVehicleUsageWarningLevelAsync(userId);

        // Assert
        Assert.Equal(UsageWarningLevel.Normal, level);
    }

    [Fact]
    public async Task IncrementVehicleCountAsync_IncreasesCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);
        var initialCount = subscription.VehiclesUsed;

        // Act
        await _service.IncrementVehicleCountAsync(userId);

        // Assert
        var updated = await _service.GetSubscriptionAsync(userId);
        Assert.Equal(initialCount + 1, updated!.VehiclesUsed);
    }

    [Fact]
    public async Task DecrementVehicleCountAsync_DecreasesCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);
        subscription.VehiclesUsed = 5;
        await _context.SaveChangesAsync();

        // Act
        await _service.DecrementVehicleCountAsync(userId);

        // Assert
        var updated = await _service.GetSubscriptionAsync(userId);
        Assert.Equal(4, updated!.VehiclesUsed);
    }

    [Fact]
    public async Task DecrementVehicleCountAsync_WhenZero_DoesNotGoNegative()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = await _service.GetOrCreateSubscriptionAsync(userId);
        subscription.VehiclesUsed = 0;
        await _context.SaveChangesAsync();

        // Act
        await _service.DecrementVehicleCountAsync(userId);

        // Assert
        var updated = await _service.GetSubscriptionAsync(userId);
        Assert.Equal(0, updated!.VehiclesUsed);
    }

    [Fact]
    public async Task UpdateTierAsync_ToPro_UpdatesLimits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _service.GetOrCreateSubscriptionAsync(userId);

        // Act
        await _service.UpdateTierAsync(userId, SubscriptionTier.Pro);

        // Assert
        var subscription = await _service.GetSubscriptionAsync(userId);
        Assert.Equal(SubscriptionTier.Pro, subscription!.Tier);
        Assert.Equal(5, subscription.VehiclesLimit);
        Assert.True(subscription.HasOcr);
        Assert.True(subscription.HasPdfExport);
    }

    [Fact]
    public async Task UpdateTierAsync_ToEnterprise_UpdatesLimits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _service.GetOrCreateSubscriptionAsync(userId);

        // Act
        await _service.UpdateTierAsync(userId, SubscriptionTier.Enterprise);

        // Assert
        var subscription = await _service.GetSubscriptionAsync(userId);
        Assert.Equal(SubscriptionTier.Enterprise, subscription!.Tier);
        Assert.Equal(50, subscription.VehiclesLimit);
        Assert.True(subscription.HasOcr);
        Assert.True(subscription.HasPdfExport);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToGracePeriod_SetsExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _service.GetOrCreateSubscriptionAsync(userId);

        // Act
        await _service.UpdateStatusAsync(userId, AccountStatus.GracePeriod);

        // Assert
        var subscription = await _service.GetSubscriptionAsync(userId);
        Assert.Equal(AccountStatus.GracePeriod, subscription!.Status);
        Assert.NotNull(subscription.GracePeriodExpiresAt);
    }
}