using FleetFuel.Data;
using FleetFuel.Api.Repositories;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service interface for Trip business logic.
/// </summary>
public interface ITripService
{
    Task<IEnumerable<Trip>> GetAllAsync(Guid userId);
    Task<IEnumerable<Trip>> GetByVehicleIdAsync(Guid vehicleId, Guid userId);
    Task<Trip?> GetByIdAsync(Guid id, Guid userId);
    Task<Trip> CreateAsync(CreateTripRequest request, Guid userId);
    Task<Trip?> UpdateAsync(Guid id, UpdateTripRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}

/// <summary>
/// Request DTOs.
/// </summary>
public record CreateTripRequest(
    Guid VehicleId,
    DateTime Date,
    int StartKm,
    int EndKm,
    string? Purpose,
    bool IsBusiness
);

public record UpdateTripRequest(
    DateTime Date,
    int StartKm,
    int EndKm,
    string? Purpose,
    bool IsBusiness
);

/// <summary>
/// Service implementation for Trip business logic.
/// </summary>
public class TripService : ITripService
{
    private readonly ITripRepository _repository;
    private readonly IVehicleRepository _vehicleRepository;

    public TripService(ITripRepository repository, IVehicleRepository vehicleRepository)
    {
        _repository = repository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IEnumerable<Trip>> GetAllAsync(Guid userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task<IEnumerable<Trip>> GetByVehicleIdAsync(Guid vehicleId, Guid userId)
    {
        // Verify vehicle belongs to user
        var vehicleExists = await _vehicleRepository.ExistsAsync(vehicleId, userId);
        if (!vehicleExists)
        {
            throw new KeyNotFoundException("Vehicle not found");
        }

        return await _repository.GetByVehicleIdAsync(vehicleId, userId);
    }

    public async Task<Trip?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _repository.GetByIdAsync(id, userId);
    }

    public async Task<Trip> CreateAsync(CreateTripRequest request, Guid userId)
    {
        // Validate vehicle exists and belongs to user
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, userId);
        if (vehicle == null)
        {
            throw new KeyNotFoundException("Vehicle not found");
        }

        // Validate StartKm >= vehicle's initial mileage
        if (request.StartKm < vehicle.InitialMileage)
        {
            throw new ArgumentException($"Start KM must be greater than or equal to vehicle's initial mileage ({vehicle.InitialMileage})");
        }

        // Validate EndKm > StartKm
        if (request.EndKm <= request.StartKm)
        {
            throw new ArgumentException("End KM must be greater than Start KM");
        }

        // Check for overlapping trips
        var hasOverlap = await CheckForOverlappingTripsAsync(request.VehicleId, userId, request.StartKm, request.EndKm);
        if (hasOverlap)
        {
            throw new ArgumentException("Trip overlaps with existing trip for this vehicle");
        }

        var trip = new Trip
        {
            UserId = userId,
            VehicleId = request.VehicleId,
            Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc),
            StartKm = request.StartKm,
            EndKm = request.EndKm,
            Purpose = request.Purpose,
            IsBusiness = request.IsBusiness
        };

        return await _repository.CreateAsync(trip);
    }

    public async Task<Trip?> UpdateAsync(Guid id, UpdateTripRequest request, Guid userId)
    {
        var trip = await _repository.GetByIdAsync(id, userId);
        if (trip == null)
        {
            return null;
        }

        // Validate EndKm > StartKm
        if (request.EndKm <= request.StartKm)
        {
            throw new ArgumentException("End KM must be greater than Start KM");
        }

        trip.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
        trip.StartKm = request.StartKm;
        trip.EndKm = request.EndKm;
        trip.Purpose = request.Purpose;
        trip.IsBusiness = request.IsBusiness;

        return await _repository.UpdateAsync(trip);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var exists = await _repository.ExistsAsync(id, userId);
        if (!exists)
        {
            return false;
        }

        await _repository.DeleteAsync(id, userId);
        return true;
    }

    private async Task<bool> CheckForOverlappingTripsAsync(Guid vehicleId, Guid userId, int startKm, int endKm)
    {
        // Check if there's any trip where the ranges overlap
        // Overlap occurs if: (NewStart < ExistingEnd) AND (NewEnd > ExistingStart)
        var existingTrips = await _repository.GetByVehicleIdAsync(vehicleId, userId);
        
        return existingTrips.Any(t => 
            (startKm < t.EndKm) && 
            (endKm > t.StartKm)
        );
    }
}
