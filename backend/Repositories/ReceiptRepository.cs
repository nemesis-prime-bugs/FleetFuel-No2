using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Repositories;

/// <summary>
/// Repository interface for Receipt operations.
/// </summary>
public interface IReceiptRepository
{
    Task<IEnumerable<Receipt>> GetAllAsync(Guid userId);
    Task<IEnumerable<Receipt>> GetByVehicleIdAsync(Guid vehicleId, Guid userId);
    Task<Receipt?> GetByIdAsync(Guid id, Guid userId);
    Task<Receipt> CreateAsync(Receipt receipt);
    Task<Receipt> UpdateAsync(Receipt receipt);
    Task DeleteAsync(Guid id, Guid userId);
    Task<bool> ExistsAsync(Guid id, Guid userId);
}

/// <summary>
/// Repository implementation for Receipt operations.
/// </summary>
public class ReceiptRepository : IReceiptRepository
{
    private readonly FleetFuelDbContext _context;

    public ReceiptRepository(FleetFuelDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Receipt>> GetAllAsync(Guid userId)
    {
        return await _context.Receipts
            .Include(r => r.Vehicle)
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Receipt>> GetByVehicleIdAsync(Guid vehicleId, Guid userId)
    {
        return await _context.Receipts
            .Where(r => r.VehicleId == vehicleId && r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    public async Task<Receipt?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Receipts
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId && !r.IsDeleted);
    }

    public async Task<Receipt> CreateAsync(Receipt receipt)
    {
        receipt.Id = Guid.NewGuid();
        receipt.CreatedAt = DateTime.UtcNow;
        receipt.ModifiedAt = DateTime.UtcNow;
        
        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();
        
        return receipt;
    }

    public async Task<Receipt> UpdateAsync(Receipt receipt)
    {
        receipt.ModifiedAt = DateTime.UtcNow;
        
        _context.Receipts.Update(receipt);
        await _context.SaveChangesAsync();
        
        return receipt;
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var receipt = await GetByIdAsync(id, userId);
        if (receipt != null)
        {
            receipt.IsDeleted = true;
            receipt.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id, Guid userId)
    {
        return await _context.Receipts
            .AnyAsync(r => r.Id == id && r.UserId == userId && !r.IsDeleted);
    }
}
