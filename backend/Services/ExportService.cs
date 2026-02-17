using System.Globalization;
using System.Text;
using System.Text.Json;
using FleetFuel.Api.Models;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Services;

/// <summary>
/// Implementation of data export operations.
/// </summary>
public class ExportService : IExportService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<ExportService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _exportStoragePath;

    private static readonly TimeSpan ExportExpiry = TimeSpan.FromHours(1);

    public ExportService(
        FleetFuelDbContext context,
        ILogger<ExportService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
        _exportStoragePath = Path.Combine(environment.ContentRootPath, "exports");

        // Ensure export directory exists
        if (!Directory.Exists(_exportStoragePath))
        {
            Directory.CreateDirectory(_exportStoragePath);
        }
    }

    public async Task<ExportStatusResponse> GenerateExportAsync(Guid userId, ExportRequest request)
    {
        try
        {
            _logger.LogInformation("Generating export for user {UserId}, format: {Format}, types: {Types}",
                userId, request.Format, request.DataTypes);

            var exportData = new Dictionary<string, object>();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new ExportStatusResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Gather requested data
            if (request.DataTypes.HasFlag(ExportDataType.Profile))
            {
                exportData["profile"] = new
                {
                    id = user.Id,
                    email = user.Email,
                    display_name = user.DisplayName,
                    bio = user.Bio,
                    created_at = user.CreatedAt,
                    updated_at = user.UpdatedAt
                };
            }

            if (request.DataTypes.HasFlag(ExportDataType.Vehicles))
            {
                var vehicles = await _context.Vehicles
                    .AsNoTracking()
                    .Where(v => v.UserId == userId && !v.IsDeleted)
                    .ToListAsync();

                exportData["vehicles"] = vehicles.Select(v => new
                {
                    id = v.Id,
                    name = v.Name,
                    license_plate = v.LicensePlate,
                    make = v.Make,
                    model = v.Model,
                    year = v.Year,
                    initial_mileage = v.InitialMileage,
                    created_at = v.CreatedAt,
                    updated_at = v.UpdatedAt
                });
            }

            if (request.DataTypes.HasFlag(ExportDataType.Trips))
            {
                var tripsQuery = _context.Trips
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && !t.IsDeleted);

                if (request.StartDate.HasValue)
                    tripsQuery = tripsQuery.Where(t => t.Date >= request.StartDate.Value);
                if (request.EndDate.HasValue)
                    tripsQuery = tripsQuery.Where(t => t.Date <= request.EndDate.Value);

                var trips = await tripsQuery.ToListAsync();

                exportData["trips"] = trips.Select(t => new
                {
                    id = t.Id,
                    vehicle_id = t.VehicleId,
                    date = t.Date,
                    distance_km = t.DistanceKm,
                    purpose = t.Purpose,
                    start_location = t.StartLocation,
                    end_location = t.EndLocation,
                    notes = t.Notes,
                    created_at = t.CreatedAt
                });
            }

            if (request.DataTypes.HasFlag(ExportDataType.Receipts))
            {
                var receiptsQuery = _context.Receipts
                    .AsNoTracking()
                    .Where(r => r.UserId == userId && !r.IsDeleted);

                if (request.StartDate.HasValue)
                    receiptsQuery = receiptsQuery.Where(r => r.Date >= request.StartDate.Value);
                if (request.EndDate.HasValue)
                    receiptsQuery = receiptsQuery.Where(r => r.Date <= request.EndDate.Value);

                var receipts = await receiptsQuery.ToListAsync();

                exportData["receipts"] = receipts.Select(r => new
                {
                    id = r.Id,
                    vehicle_id = r.VehicleId,
                    date = r.Date,
                    odometer = r.Odometer,
                    fuel_amount = r.FuelAmount,
                    price_per_unit = r.PricePerUnit,
                    total_cost = r.TotalCost,
                    fuel_station = r.FuelStation,
                    notes = r.Notes,
                    created_at = r.CreatedAt
                });
            }

            // Add metadata
            exportData["export_metadata"] = new
            {
                exported_at = DateTime.UtcNow,
                format = request.Format.ToString().ToLower(),
                data_types = request.DataTypes.ToString(),
                user_id = userId
            };

            // Generate file
            byte[] fileContent;
            string fileExtension;
            string mimeType;

            if (request.Format == ExportFormat.Json)
            {
                fileContent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
                fileExtension = ".json";
                mimeType = "application/json";
            }
            else // CSV
            {
                fileContent = GenerateCsv(exportData);
                fileExtension = ".csv";
                mimeType = "text/csv";
            }

            // Save file
            var exportId = Guid.NewGuid();
            var filename = $"fleetfuel_export_{exportId}{fileExtension}";
            var filepath = Path.Combine(_exportStoragePath, filename);
            await File.WriteAllBytesAsync(filepath, fileContent);

            var expiresAt = DateTime.UtcNow.Add(ExportExpiry);

            _logger.LogInformation("Export generated: {ExportId}, size: {Size} bytes", exportId, fileContent.Length);

            return new ExportStatusResponse
            {
                Success = true,
                DownloadUrl = $"/api/exports/{exportId}/download",
                FileSizeBytes = fileContent.Length,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate export for user {UserId}", userId);
            return new ExportStatusResponse
            {
                Success = false,
                Message = $"Export failed: {ex.Message}"
            };
        }
    }

    public async Task<byte[]?> GetExportFileAsync(Guid userId, Guid exportId)
    {
        try
        {
            var filename = $"fleetfuel_export_{exportId}.*";
            var files = Directory.GetFiles(_exportStoragePath, filename);

            foreach (var filepath in files)
            {
                var fileInfo = new FileInfo(filepath);
                if (fileInfo.CreationTime.Add(ExportExpiry) < DateTime.UtcNow)
                {
                    // File expired, delete it
                    File.Delete(filepath);
                    continue;
                }

                return await File.ReadAllBytesAsync(filepath);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get export file {ExportId} for user {UserId}", exportId, userId);
            return null;
        }
    }

    public async Task<List<ExportMetadata>> ListExportsAsync(Guid userId)
    {
        var exports = new List<ExportMetadata>();
        var files = Directory.GetFiles(_exportStoragePath, "fleetfuel_export_*.*");

        foreach (var filepath in files)
        {
            var fileInfo = new FileInfo(filepath);
            if (fileInfo.CreationTime.Add(ExportExpiry) < DateTime.UtcNow)
            {
                // File expired, skip
                continue;
            }

            // Parse export ID from filename
            var filename = fileInfo.Name;
            var parts = filename.Replace("fleetfuel_export_", "").Split('.');
            if (parts.Length < 2 || !Guid.TryParse(parts[0], out _))
            {
                continue;
            }

            exports.Add(new ExportMetadata
            {
                Id = Guid.Parse(parts[0]),
                UserId = userId,
                Format = parts[1].ToUpper(),
                DataTypes = "all",
                FileSizeBytes = fileInfo.Length,
                CreatedAt = fileInfo.CreationTime,
                ExpiresAt = fileInfo.CreationTime.Add(ExportExpiry),
                FilePath = filepath
            });
        }

        return exports.OrderByDescending(e => e.CreatedAt).ToList();
    }

    public async Task<bool> DeleteExportAsync(Guid userId, Guid exportId)
    {
        try
        {
            var filename = $"fleetfuel_export_{exportId}.*";
            var files = Directory.GetFiles(_exportStoragePath, filename);

            foreach (var filepath in files)
            {
                File.Delete(filepath);
            }

            return files.Length > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete export {ExportId} for user {UserId}", exportId, userId);
            return false;
        }
    }

    private byte[] GenerateCsv(Dictionary<string, object> data)
    {
        var sb = new StringBuilder();

        foreach (var kvp in data)
        {
            if (kvp.Value is IEnumerable<object> list && kvp.Key != "export_metadata")
            {
                sb.AppendLine($"# {kvp.Key}");
                sb.AppendLine(ConvertToCsv(list));
            }
            else if (kvp.Key == "profile")
            {
                sb.AppendLine($"# {kvp.Key}");
                sb.AppendLine(ConvertToCsv(new[] { kvp.Value }));
            }
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private string ConvertToCsv(IEnumerable<object> items)
    {
        if (!items.Any()) return "";

        var first = items.First();
        var properties = first.GetType().GetProperties();

        var header = string.Join(",", properties.Select(p => $"\"{p.Name}\""));
        var rows = items.Select(item => string.Join(",", properties.Select(p =>
        {
            var value = p.GetValue(item)?.ToString() ?? "";
            return $"\"{value.Replace("\"", "\"\"")}\"";
        })));

        return $"{header}\n{string.Join("\n", rows)}";
    }
}