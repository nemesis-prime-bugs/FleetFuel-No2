using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Services;

/// <summary>
/// Service interface for Summary/Reporting operations.
/// </summary>
public interface ISummaryService
{
    Task<YearlySummary> GetYearlySummaryAsync(Guid userId, int year);
    Task<byte[]> ExportYearlySummaryCsvAsync(Guid userId, int year);
}

/// <summary>
/// Yearly summary DTO.
/// </summary>
public class YearlySummary
{
    public int Year { get; set; }
    public List<VehicleSummary> VehicleSummaries { get; set; } = new();
    public int TotalTrips { get; set; }
    public int TotalBusinessTrips { get; set; }
    public int TotalKm { get; set; }
    public int TotalBusinessKm { get; set; }
    public decimal TotalExpenses { get; set; }
    public int TotalReceipts { get; set; }
}

public class VehicleSummary
{
    public Guid VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int TripCount { get; set; }
    public int BusinessTripCount { get; set; }
    public int TotalKm { get; set; }
    public int BusinessKm { get; set; }
    public decimal TotalExpenses { get; set; }
    public int ReceiptCount { get; set; }
}

/// <summary>
/// Service implementation for Summary/Reporting operations.
/// </summary>
public class SummaryService : ISummaryService
{
    private readonly FleetFuelDbContext _context;

    public SummaryService(FleetFuelDbContext context)
    {
        _context = context;
    }

    public async Task<YearlySummary> GetYearlySummaryAsync(Guid userId, int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59);

        // Get all vehicles for user
        var vehicles = await _context.Vehicles
            .Where(v => v.UserId == userId && !v.IsDeleted)
            .ToListAsync();

        var vehicleSummaries = new List<VehicleSummary>();
        int totalTrips = 0;
        int totalBusinessTrips = 0;
        int totalKm = 0;
        int totalBusinessKm = 0;
        decimal totalExpenses = 0;
        int totalReceipts = 0;

        foreach (var vehicle in vehicles)
        {
            // Get trips for this vehicle in the year
            var trips = await _context.Trips
                .Where(t => t.VehicleId == vehicle.Id && 
                            t.UserId == userId && 
                            !t.IsDeleted &&
                            t.Date >= startDate && 
                            t.Date <= endDate)
                .ToListAsync();

            var vehicleKm = trips.Sum(t => t.EndKm - t.StartKm);
            var vehicleBusinessKm = trips.Where(t => t.IsBusiness).Sum(t => t.EndKm - t.StartKm);

            // Get receipts for this vehicle in the year
            var receipts = await _context.Receipts
                .Where(r => r.VehicleId == vehicle.Id && 
                            r.UserId == userId && 
                            !r.IsDeleted &&
                            r.Date >= startDate && 
                            r.Date <= endDate)
                .ToListAsync();

            var vehicleExpenses = receipts.Sum(r => r.Amount);

            vehicleSummaries.Add(new VehicleSummary
            {
                VehicleId = vehicle.Id,
                VehicleName = vehicle.Name,
                LicensePlate = vehicle.LicensePlate,
                TripCount = trips.Count,
                BusinessTripCount = trips.Count(t => t.IsBusiness),
                TotalKm = vehicleKm,
                BusinessKm = vehicleBusinessKm,
                TotalExpenses = vehicleExpenses,
                ReceiptCount = receipts.Count,
            });

            totalTrips += trips.Count;
            totalBusinessTrips += trips.Count(t => t.IsBusiness);
            totalKm += vehicleKm;
            totalBusinessKm += vehicleBusinessKm;
            totalExpenses += vehicleExpenses;
            totalReceipts += receipts.Count;
        }

        return new YearlySummary
        {
            Year = year,
            VehicleSummaries = vehicleSummaries,
            TotalTrips = totalTrips,
            TotalBusinessTrips = totalBusinessTrips,
            TotalKm = totalKm,
            TotalBusinessKm = totalBusinessKm,
            TotalExpenses = totalExpenses,
            TotalReceipts = totalReceipts,
        };
    }

    public async Task<byte[]> ExportYearlySummaryCsvAsync(Guid userId, int year)
    {
        var summary = await GetYearlySummaryAsync(userId, year);

        var sb = new System.Text.StringBuilder();

        // Header
        sb.AppendLine("FleetFuel Yearly Summary Report");
        sb.AppendLine($"Year,{summary.Year}");
        sb.AppendLine($"Generated,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Totals
        sb.AppendLine("TOTALS");
        sb.AppendLine($"Total Vehicles,{summary.VehicleSummaries.Count}");
        sb.AppendLine($"Total Trips,{summary.TotalTrips}");
        sb.AppendLine($"Total Business Trips,{summary.TotalBusinessTrips}");
        sb.AppendLine($"Total Kilometers,{summary.TotalKm}");
        sb.AppendLine($"Total Business Kilometers,{summary.TotalBusinessKm}");
        sb.AppendLine($"Total Expenses,€ {summary.TotalExpenses:F2}");
        sb.AppendLine($"Total Receipts,{summary.TotalReceipts}");
        sb.AppendLine();

        // Per-vehicle breakdown
        sb.AppendLine("VEHICLE BREAKDOWN");
        sb.AppendLine("Vehicle Name,License Plate,Trips,Business Trips,Total KM,Business KM,Expenses,Receipts");

        foreach (var vs in summary.VehicleSummaries)
        {
            sb.AppendLine($"{vs.VehicleName},{vs.LicensePlate},{vs.TripCount},{vs.BusinessTripCount},{vs.TotalKm},{vs.BusinessKm},€ {vs.TotalExpenses:F2},{vs.ReceiptCount}");
        }

        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }
}