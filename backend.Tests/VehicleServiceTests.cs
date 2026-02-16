using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FleetFuel.Api.Tests;

public class VehicleServiceTests : IDisposable
{
    private readonly FleetFuelDbContext _context;
    private readonly VehicleService _service;
    private readonly IVehicleRepository _repository;

    public VehicleServiceTests()
    {
        // Use in-memory database for tests
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new FleetFuelDbContext(options);
        _repository = new VehicleRepository(_context);
        _service = new VehicleService(_repository);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_ReturnsVehicle()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateVehicleRequest("Test Car", "AB-123-CD", 50000);

        // Act
        var result = await _service.CreateAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Car", result.Name);
        Assert.Equal("AB-123-CD", result.LicensePlate);
        Assert.Equal(50000, result.InitialMileage);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task CreateVehicle_UppercasesLicensePlate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateVehicleRequest("Test Car", "ab-123-cd", 50000);

        // Act
        var result = await _service.CreateAsync(request, userId);

        // Assert
        Assert.Equal("AB-123-CD", result.LicensePlate);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyUserVehicles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await _service.CreateAsync(new CreateVehicleRequest("User1 Car", "AA-111-A", 10000), userId);
        await _service.CreateAsync(new CreateVehicleRequest("User2 Car", "BB-222-B", 20000), otherUserId);

        // Act
        var result = await _service.GetAllAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal("User1 Car", result.First().Name);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesVehicle()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicle = await _service.CreateAsync(
            new CreateVehicleRequest("Original Name", "CC-333-C", 30000), userId);

        var updateRequest = new UpdateVehicleRequest("Updated Name", "DD-444-D", 35000);
        var result = await _service.UpdateAsync(vehicle.Id, updateRequest, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result!.Name);
        Assert.Equal("DD-444-D", result.LicensePlate);
        Assert.Equal(35000, result.InitialMileage);
    }

    [Fact]
    public async Task DeleteAsync_MarksAsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicle = await _service.CreateAsync(
            new CreateVehicleRequest("To Delete", "EE-555-E", 40000), userId);

        // Act
        var deleted = await _service.DeleteAsync(vehicle.Id, userId);
        var allVehicles = await _service.GetAllAsync(userId);

        // Assert
        Assert.True(deleted);
        Assert.Empty(allVehicles);
    }

    [Fact]
    public async Task DeleteAsync_WithWrongUserId_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var vehicle = await _service.CreateAsync(
            new CreateVehicleRequest("Other User Car", "FF-666-F", 60000), userId);

        // Act
        var deleted = await _service.DeleteAsync(vehicle.Id, otherUserId);

        // Assert
        Assert.False(deleted);
    }
}
