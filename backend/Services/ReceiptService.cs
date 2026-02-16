using FleetFuel.Data;
using FleetFuel.Api.Repositories;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service interface for Receipt business logic.
/// </summary>
public interface IReceiptService
{
    Task<IEnumerable<Receipt>> GetAllAsync(Guid userId);
    Task<IEnumerable<Receipt>> GetByVehicleIdAsync(Guid vehicleId, Guid userId);
    Task<Receipt?> GetByIdAsync(Guid id, Guid userId);
    Task<Receipt> CreateAsync(CreateReceiptRequest request, Guid userId, string? imagePath = null);
    Task<Receipt?> UpdateAsync(Guid id, UpdateReceiptRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}

/// <summary>
/// Request DTOs.
/// </summary>
public record CreateReceiptRequest(
    Guid VehicleId,
    DateTime Date,
    decimal Amount,
    decimal? FuelLiters,
    string? FuelType,
    string? StationName
);

public record UpdateReceiptRequest(
    DateTime Date,
    decimal Amount,
    decimal? FuelLiters,
    string? FuelType,
    string? StationName
);

/// <summary>
/// Service implementation for Receipt business logic.
/// </summary>
public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _repository;
    private readonly IVehicleRepository _vehicleRepository;

    public ReceiptService(IReceiptRepository repository, IVehicleRepository vehicleRepository)
    {
        _repository = repository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IEnumerable<Receipt>> GetAllAsync(Guid userId)
    {
        return await _repository.GetAllAsync(userId);
    }

    public async Task<IEnumerable<Receipt>> GetByVehicleIdAsync(Guid vehicleId, Guid userId)
    {
        // Verify vehicle belongs to user
        var vehicleExists = await _vehicleRepository.ExistsAsync(vehicleId, userId);
        if (!vehicleExists)
        {
            throw new KeyNotFoundException("Vehicle not found");
        }

        return await _repository.GetByVehicleIdAsync(vehicleId, userId);
    }

    public async Task<Receipt?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _repository.GetByIdAsync(id, userId);
    }

    public async Task<Receipt> CreateAsync(CreateReceiptRequest request, Guid userId, string? imagePath = null)
    {
        // Validate vehicle exists and belongs to user
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, userId);
        if (vehicle == null)
        {
            throw new KeyNotFoundException("Vehicle not found");
        }

        // Validate amount
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero");
        }

        var receipt = new Receipt
        {
            UserId = userId,
            VehicleId = request.VehicleId,
            Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc),
            Amount = request.Amount,
            FuelLiters = request.FuelLiters,
            FuelType = request.FuelType?.ToLowerInvariant(),
            StationName = request.StationName,
            ImagePath = imagePath
        };

        return await _repository.CreateAsync(receipt);
    }

    public async Task<Receipt?> UpdateAsync(Guid id, UpdateReceiptRequest request, Guid userId)
    {
        var receipt = await _repository.GetByIdAsync(id, userId);
        if (receipt == null)
        {
            return null;
        }

        // Validate amount
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero");
        }

        receipt.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
        receipt.Amount = request.Amount;
        receipt.FuelLiters = request.FuelLiters;
        receipt.FuelType = request.FuelType?.ToLowerInvariant();
        receipt.StationName = request.StationName;

        return await _repository.UpdateAsync(receipt);
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
}
