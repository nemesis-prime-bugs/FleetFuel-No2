using FleetFuel.Api.Models;
using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FleetFuel.Api.Tests.Services;

/// <summary>
/// Unit tests for UserService.
/// </summary>
public class UserServiceTests : IDisposable
{
    private readonly FleetFuelDbContext _context;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly UserService _service;
    private readonly Guid _testUserId;

    public UserServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserServiceTests_{Guid.NewGuid()}")
            .Options;
        _context = new FleetFuelDbContext(options);

        // Setup mocks
        _loggerMock = new Mock<ILogger<UserService>>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _environmentMock.Setup(e => e.ContentRootPath).Returns("/tmp");

        _service = new UserService(_context, _loggerMock.Object, _environmentMock.Object);

        // Create test user
        _testUserId = Guid.NewGuid();
        var testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Bio = "Test bio",
            AvatarUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Users.Add(testUser);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetProfileAsync Tests

    [Fact]
    public async Task GetProfileAsync_WithValidUserId_ReturnsProfile()
    {
        // Act
        var result = await _service.GetProfileAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testUserId, result.Id);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.DisplayName);
        Assert.Equal("Test bio", result.Bio);
    }

    [Fact]
    public async Task GetProfileAsync_WithInvalidUserId_ReturnsNull()
    {
        // Act
        var result = await _service.GetProfileAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProfileAsync_WithDeletedUser_ReturnsNull()
    {
        // Arrange
        var deletedUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "deleted@example.com",
            IsDeleted = true
        };
        _context.Users.Add(deletedUser);
        _context.SaveChanges();

        // Act
        var result = await _service.GetProfileAsync(deletedUser.Id);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_WithValidRequest_UpdatesProfile()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            DisplayName = "Updated Name",
            Bio = "Updated bio"
        };

        // Act
        var result = await _service.UpdateProfileAsync(_testUserId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.DisplayName);
        Assert.Equal("Updated bio", result.Bio);

        // Verify database was updated
        var dbUser = await _context.Users.FindAsync(_testUserId);
        Assert.Equal("Updated Name", dbUser?.DisplayName);
        Assert.Equal("Updated bio", dbUser?.Bio);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithOnlyDisplayName_UpdatesDisplayName()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            DisplayName = "New Name",
            Bio = null
        };

        // Act
        var result = await _service.UpdateProfileAsync(_testUserId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.DisplayName);
        Assert.Equal("Test bio", result.Bio); // Original bio preserved
    }

    [Fact]
    public async Task UpdateProfileAsync_WithInvalidUserId_ReturnsNull()
    {
        // Arrange
        var request = new UpdateProfileRequest { DisplayName = "Test" };

        // Act
        var result = await _service.UpdateProfileAsync(Guid.NewGuid(), request);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region UploadAvatarAsync Tests

    [Fact]
    public async Task UploadAvatarAsync_WithValidData_UploadsAvatar()
    {
        // Arrange
        var avatarData = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        var request = new UploadAvatarRequest
        {
            AvatarBase64 = avatarData,
            FileExtension = ".png"
        };

        // Act
        var result = await _service.UploadAvatarAsync(_testUserId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("/avatars/", result.AvatarUrl);
        Assert.EndsWith(".png", result.AvatarUrl);
    }

    [Fact]
    public async Task UploadAvatarAsync_WithInvalidExtension_ThrowsArgumentException()
    {
        // Arrange
        var request = new UploadAvatarRequest
        {
            AvatarBase64 = "SGVsbG8=",
            FileExtension = ".exe"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadAvatarAsync(_testUserId, request));
    }

    [Fact]
    public async Task UploadAvatarAsync_WithInvalidUserId_ReturnsNull()
    {
        // Arrange
        var request = new UploadAvatarRequest
        {
            AvatarBase64 = "SGVsbG8=",
            FileExtension = ".png"
        };

        // Act
        var result = await _service.UploadAvatarAsync(Guid.NewGuid(), request);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region RemoveAvatarAsync Tests

    [Fact]
    public async Task RemoveAvatarAsync_WithExistingAvatar_RemovesAvatar()
    {
        // Arrange - First upload an avatar
        var avatarData = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        await _service.UploadAvatarAsync(_testUserId, new UploadAvatarRequest
        {
            AvatarBase64 = avatarData,
            FileExtension = ".png"
        });

        // Act
        var result = await _service.RemoveAvatarAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.AvatarUrl);
    }

    [Fact]
    public async Task RemoveAvatarAsync_WithInvalidUserId_ReturnsNull()
    {
        // Act
        var result = await _service.RemoveAvatarAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetPreferencesAsync Tests

    [Fact]
    public async Task GetPreferencesAsync_WithNoPreferences_ReturnsDefault()
    {
        // Act
        var result = await _service.GetPreferencesAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.Currency);
        Assert.Equal("km", result.DistanceUnit);
        Assert.Equal("system", result.Theme);
    }

    [Fact]
    public async Task GetPreferencesAsync_WithExistingPreferences_ReturnsPreferences()
    {
        // Arrange - First update preferences
        var updateRequest = new UpdatePreferencesRequest
        {
            Preferences = new UserPreferences
            {
                Currency = "EUR",
                DistanceUnit = "mi",
                Theme = "dark"
            }
        };
        await _service.UpdatePreferencesAsync(_testUserId, updateRequest);

        // Act
        var result = await _service.GetPreferencesAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal("mi", result.DistanceUnit);
        Assert.Equal("dark", result.Theme);
    }

    [Fact]
    public async Task GetPreferencesAsync_WithInvalidUserId_ReturnsNull()
    {
        // Act
        var result = await _service.GetPreferencesAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region UpdatePreferencesAsync Tests

    [Fact]
    public async Task UpdatePreferencesAsync_WithValidRequest_UpdatesPreferences()
    {
        // Arrange
        var request = new UpdatePreferencesRequest
        {
            Preferences = new UserPreferences
            {
                Currency = "GBP",
                DistanceUnit = "mi",
                VolumeUnit = "gal_us",
                Theme = "light"
            }
        };

        // Act
        var result = await _service.UpdatePreferencesAsync(_testUserId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GBP", result.Currency);
        Assert.Equal("mi", result.DistanceUnit);
        Assert.Equal("gal_us", result.VolumeUnit);
        Assert.Equal("light", result.Theme);
    }

    [Fact]
    public async Task UpdatePreferencesAsync_PartialUpdate_MergesPreferences()
    {
        // Arrange - Set initial preferences
        await _service.UpdatePreferencesAsync(_testUserId, new UpdatePreferencesRequest
        {
            Preferences = new UserPreferences
            {
                Currency = "USD",
                DistanceUnit = "km",
                Theme = "system"
            }
        });

        // Act - Update only currency
        var result = await _service.UpdatePreferencesAsync(_testUserId, new UpdatePreferencesRequest
        {
            Preferences = new UserPreferences
            {
                Currency = "EUR"
            }
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal("km", result.DistanceUnit); // Preserved
        Assert.Equal("system", result.Theme); // Preserved
    }

    [Fact]
    public async Task UpdatePreferencesAsync_WithInvalidUserId_ReturnsNull()
    {
        // Arrange
        var request = new UpdatePreferencesRequest
        {
            Preferences = new UserPreferences { Currency = "EUR" }
        };

        // Act
        var result = await _service.UpdatePreferencesAsync(Guid.NewGuid(), request);

        // Assert
        Assert.Null(result);
    }

    #endregion
}