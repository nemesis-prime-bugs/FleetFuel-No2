using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Repositories;

/// <summary>
/// Repository interface for Trip operations.
/// </summary>
public interface ITripRepository
{
    Task<IEnumerable<Trip>> GetAllAsync(Guid userId);
    Task<IEnumerable<Trip>> GetByVehicleIdAsync(Guid vehicleId, Guid userId);
    Task<Trip?> GetByIdAsync(Guid id, Guid userId);
    Task<Trip> CreateAsync(Trip trip);
    Task<Trip> UpdateAsync(Trip trip);
    Task DeleteAsync(Guid id, Guid userId);
    Task<bool> ExistsAsync(Guid id, Guid userId);
}

/// <summary>
/// Repository implementation for Trip operations.
/// </summary>
public class TripRepository : ITripRepository
{
    private readonly FleetFuelDbContext _context;

    public TripRepository(FleetFuelDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Trip>> GetAllAsync(Guid userId)
    {
        return await _context.Trips
            .Include(t => t.Vehicle)
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Trip>> GetByVehicleIdAsync(Guid vehicleId, Guid userId)
    {
        return await _context.Trips
            .Where(t => t.VehicleId == vehicleId && t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<Trip?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Trips
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
    }

    public async Task<Trip> CreateAsync(Trip trip)
    {
        trip.Id = Guid.NewGuid();
        trip.CreatedAt = DateTime.UtcNow;
        trip.ModifiedAt = DateTime.UtcNow;
        
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
        
        return trip;
    }

    public async Task<Trip> UpdateAsync(Trip trip)
    {
        trip.ModifiedAt = DateTime.UtcNow;
        
        _context.Trips.Update(trip);
        await _context.SaveChangesAsync();
        
        return trip;
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var trip = await GetByIdAsync(id, userId);
        if (trip != null)
        {
            trip.IsDeleted = true;
            trip.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id, Guid userId)
    {
        return await _context.Trips
            .AnyAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
    }
}
