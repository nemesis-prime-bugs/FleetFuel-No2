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
/// Request DTOs.
/// </summary>
public record CreateVehicleRequest(string Name, string LicensePlate, int InitialMileage);
public record UpdateVehicleRequest(string Name, string LicensePlate, int InitialMileage);

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
            Name = request.Name,
            LicensePlate = request.LicensePlate.ToUpperInvariant(),
            InitialMileage = request.InitialMileage
        };

        return await _repository.CreateAsync(vehicle);
    }

    public async Task<Vehicle?> UpdateAsync(Guid id, UpdateVehicleRequest request, Guid userId)
    {
        var vehicle = await _repository.GetByIdAsync(id, userId);
        if (vehicle == null) return null;

        vehicle.Name = request.Name;
        vehicle.LicensePlate = request.LicensePlate.ToUpperInvariant();
        vehicle.InitialMileage = request.InitialMileage;

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