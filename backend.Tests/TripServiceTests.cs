using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FleetFuel.Api.Tests;

public class TripServiceTests : IDisposable
{
    private readonly FleetFuelDbContext _context;
    private readonly TripService _service;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITripRepository _tripRepository;
    private Guid _userId;
    private Guid _vehicleId;

    public TripServiceTests()
    {
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"TripTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new FleetFuelDbContext(options);
        _vehicleRepository = new VehicleRepository(_context);
        _tripRepository = new TripRepository(_context);
        _service = new TripService(_tripRepository, _vehicleRepository);

        _userId = Guid.NewGuid();
        _vehicleId = CreateTestVehicle();
    }

    private Guid CreateTestVehicle()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Test Vehicle",
            LicensePlate = "TEST-001",
            InitialMileage = 10000,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
        return vehicle.Id;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateTrip_WithValidData_ReturnsTrip()
    {
        // Arrange
        var request = new CreateTripRequest(
            _vehicleId,
            DateTime.UtcNow,
            10000,
            10500,
            "Test trip",
            true
        );

        // Act
        var result = await _service.CreateAsync(request, _userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10000, result.StartKm);
        Assert.Equal(10500, result.EndKm);
        Assert.Equal(500, result.EndKm - result.StartKm);
    }

    [Fact]
    public async Task CreateTrip_WithEndKmLessThanStartKm_Throws()
    {
        // Arrange
        var request = new CreateTripRequest(
            _vehicleId,
            DateTime.UtcNow,
            10500,
            10000, // End less than start
            "Invalid trip",
            true
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(request, _userId));
    }

    [Fact]
    public async Task CreateTrip_WithStartKmLessThanVehicleInitial_Throws()
    {
        // Arrange - Start KM less than vehicle's initial mileage (10000)
        var request = new CreateTripRequest(
            _vehicleId,
            DateTime.UtcNow,
            5000, // Less than initial 10000
            11000,
            "Invalid start km",
            true
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(request, _userId));
    }

    [Fact]
    public async Task CreateTrip_WithOverlappingKm_Throws()
    {
        // Arrange - Create first trip
        await _service.CreateAsync(new CreateTripRequest(
            _vehicleId, DateTime.UtcNow, 10000, 10500, "First trip", true), _userId);

        // Act & Assert - Second trip overlaps
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(new CreateTripRequest(
                _vehicleId, DateTime.UtcNow, 10300, 11000, "Overlapping trip", true), _userId));
    }

    [Fact]
    public async Task CreateTrip_WithNonExistentVehicle_Throws()
    {
        // Arrange
        var nonExistentVehicleId = Guid.NewGuid();
        var request = new CreateTripRequest(
            nonExistentVehicleId,
            DateTime.UtcNow,
            10000,
            10500,
            "Test trip",
            true
        );

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateAsync(request, _userId));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUserTrips()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var otherVehicleId = CreateTestVehicle();

        await _service.CreateAsync(new CreateTripRequest(
            _vehicleId, DateTime.UtcNow, 10000, 10500, "My trip", true), _userId);

        await _service.CreateAsync(new CreateTripRequest(
            otherVehicleId, DateTime.UtcNow, 20000, 20500, "Other trip", true), otherUserId);

        // Act
        var myTrips = await _service.GetAllAsync(_userId);

        // Assert
        Assert.Single(myTrips);
        Assert.Equal("My trip", myTrips.First().Purpose);
    }

    [Fact]
    public async Task GetByVehicleIdAsync_ReturnsVehicleTrips()
    {
        // Arrange
        await _service.CreateAsync(new CreateTripRequest(
            _vehicleId, DateTime.UtcNow, 10000, 10500, "Trip 1", true), _userId);
        await _service.CreateAsync(new CreateTripRequest(
            _vehicleId, DateTime.UtcNow, 10500, 11000, "Trip 2", false), _userId);

        // Act
        var vehicleTrips = await _service.GetByVehicleIdAsync(_vehicleId, _userId);

        // Assert
        Assert.Equal(2, vehicleTrips.Count());
    }

    [Fact]
    public async Task DeleteAsync_MarksAsDeleted()
    {
        // Arrange
        var trip = await _service.CreateAsync(new CreateTripRequest(
            _vehicleId, DateTime.UtcNow, 10000, 10500, "To delete", true), _userId);

        // Act
        var deleted = await _service.DeleteAsync(trip.Id, _userId);
        var allTrips = await _service.GetAllAsync(_userId);

        // Assert
        Assert.True(deleted);
        Assert.Empty(allTrips);
    }
}
