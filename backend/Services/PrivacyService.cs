using FleetFuel.Api.Models;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Api.Services;

/// <summary>
/// Implementation of privacy and data management operations.
/// </summary>
public class PrivacyService : IPrivacyService
{
    private readonly FleetFuelDbContext _context;
    private readonly ILogger<PrivacyService> _logger;
    private readonly IExportService _exportService;

    public PrivacyService(
        FleetFuelDbContext context,
        ILogger<PrivacyService> logger,
        IExportService exportService)
    {
        _context = context;
        _logger = logger;
        _exportService = exportService;
    }

    public async Task<PrivacySettings?> GetPrivacySettingsAsync(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        // Parse preferences JSON for privacy settings
        if (string.IsNullOrEmpty(user.PreferencesJson))
        {
            return new PrivacySettings();
        }

        try
        {
            // Extract privacy portion from preferences
            var preferences = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonDocument>(user.PreferencesJson);
            var root = preferences.RootElement;

            return new PrivacySettings
            {
                ShareAnalytics = root.TryGetProperty("shareAnalytics", out var analytics) ? analytics.GetBoolean() : false,
                AllowPersonalizedAds = root.TryGetProperty("allowPersonalizedAds", out var ads) ? ads.GetBoolean() : false,
                EnableCookies = root.TryGetProperty("enableCookies", out var cookies) ? cookies.GetBoolean() : true,
                AllowThirdPartySharing = root.TryGetProperty("allowThirdPartySharing", out var sharing) ? sharing.GetBoolean() : false,
                ShowProfilePublicly = root.TryGetProperty("showProfilePublicly", out var publicProfile) ? publicProfile.GetBoolean() : false,
            };
        }
        catch
        {
            return new PrivacySettings();
        }
    }

    public async Task<PrivacySettings?> UpdatePrivacySettingsAsync(Guid userId, UpdatePrivacyRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        // Parse existing preferences
        var preferences = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(user.PreferencesJson))
        {
            try
            {
                preferences = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.PreferencesJson) 
                    ?? new Dictionary<string, object>();
            }
            catch
            {
                preferences = new Dictionary<string, object>();
            }
        }

        // Update privacy settings
        if (request.Settings != null)
        {
            preferences["shareAnalytics"] = request.Settings.ShareAnalytics;
            preferences["allowPersonalizedAds"] = request.Settings.AllowPersonalizedAds;
            preferences["enableCookies"] = request.Settings.EnableCookies;
            preferences["allowThirdPartySharing"] = request.Settings.AllowThirdPartySharing;
            preferences["showProfilePublicly"] = request.Settings.ShowProfilePublicly;
        }

        user.PreferencesJson = System.Text.Json.JsonSerializer.Serialize(preferences);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Privacy settings updated for user: {UserId}", userId);

        return request.Settings ?? new PrivacySettings();
    }

    public async Task<DataExportStatus> RequestDataExportAsync(Guid userId)
    {
        _logger.LogInformation("Data export requested for user: {UserId}", userId);

        var exportRequest = new ExportRequest
        {
            Format = ExportFormat.Json,
            DataTypes = ExportDataType.All
        };

        var result = await _exportService.GenerateExportAsync(userId, exportRequest);

        return new DataExportStatus
        {
            Success = result.Success,
            DownloadUrl = result.DownloadUrl,
            AvailableUntil = result.ExpiresAt,
            Message = result.Message
        };
    }

    public async Task<DeleteAccountResponse> RequestAccountDeletionAsync(Guid userId, DeleteAccountRequest request)
    {
        _logger.LogWarning("Account deletion requested for user: {UserId}", userId);

        if (request.Confirmation.ToLower() != "delete my account")
        {
            return new DeleteAccountResponse
            {
                Success = false,
                Message = "Confirmation text does not match. Please type 'delete my account' to confirm."
            };
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new DeleteAccountResponse
            {
                Success = false,
                Message = "User not found"
            };
        }

        // For MVP, mark user for deletion
        // In production, this would:
        // 1. Export data if requested
        // 2. Cancel any Stripe subscriptions
        // 3. Delete from Supabase Auth
        // 4. Anonymize data after grace period
        
        user.IsDeleted = true;
        user.Email = $"deleted_{Guid.NewGuid()}@example.com"; // Anonymize email
        user.DisplayName = "Deleted User";
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new DeleteAccountResponse
        {
            Success = true,
            Message = "Your account has been scheduled for deletion. You have 30 days to cancel.",
            DeletionCompletedAt = DateTime.UtcNow.AddDays(30)
        };
    }

    public async Task<bool> CancelAccountDeletionAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null || !user.IsDeleted)
        {
            return false;
        }

        // Restore user
        user.IsDeleted = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Account deletion cancelled for user: {UserId}", userId);

        return true;
    }

    public async Task<bool> AnonymizeDataAsync(Guid userId)
    {
        try
        {
            // Anonymize all user data
            var vehicles = await _context.Vehicles
                .Where(v => v.UserId == userId)
                .ToListAsync();

            foreach (var vehicle in vehicles)
            {
                vehicle.Name = "Deleted Vehicle";
                vehicle.LicensePlate = "XXX-XXX";
            }

            // Anonymize trips
            var trips = await _context.Trips
                .Where(t => t.UserId == userId)
                .ToListAsync();

            foreach (var trip in trips)
            {
                trip.StartLocation = "Deleted";
                trip.EndLocation = "Deleted";
                trip.Notes = "Deleted";
            }

            // Anonymize receipts
            var receipts = await _context.Receipts
                .Where(r => r.UserId == userId)
                .ToListAsync();

            foreach (var receipt in receipts)
            {
                receipt.FuelStation = "Deleted";
                receipt.Notes = "Deleted";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User data anonymized for: {UserId}", userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to anonymize data for user: {UserId}", userId);
            return false;
        }
    }
}