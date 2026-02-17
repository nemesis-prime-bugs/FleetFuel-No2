using FleetFuel.Data;
using FleetFuel.Api.Repositories;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service interface for Vehicle business logic.
/// </summary>
public interface IVehicleService
{
    Task<IEnumerable<Vehicle>> GetAllAsync(Guid userId);
    Task<Vehicle?> GetByIdAsync(Guid id, Guid userId);
    Task<Vehicle> CreateAsync(CreateVehicleRequest request, Guid userId);
    Task<Vehicle?> UpdateAsync(Guid id, UpdateVehicleRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}

/// <summary>
/// Request DTOs - use lowercase snake_case to match frontend JSON
/// </summary>
public class CreateVehicleRequest
{
    public string name { get; set; } = string.Empty;
    public string license_plate { get; set; } = string.Empty;
    public int initial_mileage { get; set; }
}

public class UpdateVehicleRequest
{
    public string name { get; set; } = string.Empty;
    public string license_plate { get; set; } = string.Empty;
    public int initial_mileage { get; set; }
}

/// <summary>
/// Service implementation for Vehicle business logic.
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _repository;

    public VehicleService(IVehicleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync(Guid userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _repository.GetByIdAsync(id, userId);
    }

    public async Task<Vehicle> CreateAsync(CreateVehicleRequest request, Guid userId)
    {
        var vehicle = new Vehicle
        {
            UserId = userId,
            Name = request.name,
            LicensePlate = request.license_plate.ToUpperInvariant(),
            InitialMileage = request.initial_mileage
        };

        return await _repository.CreateAsync(vehicle);
    }

    public async Task<Vehicle?> UpdateAsync(Guid id, UpdateVehicleRequest request, Guid userId)
    {
        var vehicle = await _repository.GetByIdAsync(id, userId);
        if (vehicle == null) return null;

        vehicle.Name = request.name;
        vehicle.LicensePlate = request.license_plate.ToUpperInvariant();
        vehicle.InitialMileage = request.initial_mileage;

        return await _repository.UpdateAsync(vehicle);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var exists = await _repository.ExistsAsync(id, userId);
        if (!exists) return false;

        await _repository.DeleteAsync(id, userId);
        return true;
    }
}