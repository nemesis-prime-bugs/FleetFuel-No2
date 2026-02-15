using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Repositories;

/// <summary>
/// Repository interface for Vehicle operations.
/// </summary>
public interface IVehicleRepository
{
    Task<IEnumerable<Vehicle>> GetAllAsync(Guid userId);
    Task<Vehicle?> GetByIdAsync(Guid id, Guid userId);
    Task<Vehicle> CreateAsync(Vehicle vehicle);
    Task<Vehicle> UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(Guid id, Guid userId);
    Task<bool> ExistsAsync(Guid id, Guid userId);
}

/// <summary>
/// Repository implementation for Vehicle operations.
/// </summary>
public class VehicleRepository : IVehicleRepository
{
    private readonly FleetFuelDbContext _context;

    public VehicleRepository(FleetFuelDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync(Guid userId)
    {
        return await _context.Vehicles
            .Where(v => v.UserId == userId && !v.IsDeleted)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId && !v.IsDeleted);
    }

    public async Task<Vehicle> CreateAsync(Vehicle vehicle)
    {
        vehicle.Id = Guid.NewGuid();
        vehicle.CreatedAt = DateTime.UtcNow;
        vehicle.ModifiedAt = DateTime.UtcNow;
        
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        
        return vehicle;
    }

    public async Task<Vehicle> UpdateAsync(Vehicle vehicle)
    {
        vehicle.ModifiedAt = DateTime.UtcNow;
        
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
        
        return vehicle;
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var vehicle = await GetByIdAsync(id, userId);
        if (vehicle != null)
        {
            vehicle.IsDeleted = true;
            vehicle.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id, Guid userId)
    {
        return await _context.Vehicles
            .AnyAsync(v => v.Id == id && v.UserId == userId && !v.IsDeleted);
    }
}