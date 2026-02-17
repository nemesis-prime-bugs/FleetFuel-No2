using FleetFuel.Api.Models;
using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FleetFuel.Api.Tests.Services;

/// <summary>
/// Unit tests for SecurityService.
/// </summary>
public class SecurityServiceTests : IDisposable
{
    private readonly FleetFuelDbContext _context;
    private readonly Mock<ILogger<SecurityService>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly SecurityService _service;
    private readonly Guid _testUserId;

    public SecurityServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"SecurityServiceTests_{Guid.NewGuid()}")
            .Options;
        _context = new FleetFuelDbContext(options);

        // Setup mocks
        _loggerMock = new Mock<ILogger<SecurityService>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _service = new SecurityService(_context, _loggerMock.Object, _httpContextAccessorMock.Object);

        // Create test user
        _testUserId = Guid.NewGuid();
        var testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
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

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithMatchingPasswords_ReturnsSuccess()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword456",
            ConfirmPassword = "NewPassword456"
        };

        // Act
        var result = await _service.ChangePasswordAsync(_testUserId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("initiated", result.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithMismatchedPasswords_ReturnsFailure()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword456",
            ConfirmPassword = "DifferentPassword"
        };

        // Act
        var result = await _service.ChangePasswordAsync(_testUserId, request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("do not match", result.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithShortPassword_ReturnsFailure()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "Short1",
            ConfirmPassword = "Short1"
        };

        // Act
        var result = await _service.ChangePasswordAsync(_testUserId, request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("at least 8 characters", result.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithWeakPassword_ReturnsFailure()
    {
        // Arrange - Password without uppercase
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "lowercase123",
            ConfirmPassword = "lowercase123"
        };

        // Act
        var result = await _service.ChangePasswordAsync(_testUserId, request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("uppercase", result.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidUserId_ReturnsFailure()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword123",
            NewPassword = "NewPassword456",
            ConfirmPassword = "NewPassword456"
        };

        // Act
        var result = await _service.ChangePasswordAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    #endregion

    #region GetSecurityInfoAsync Tests

    [Fact]
    public async Task GetSecurityInfoAsync_WithValidUserId_ReturnsInfo()
    {
        // Act
        var result = await _service.GetSecurityInfoAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Sessions);
    }

    [Fact]
    public async Task GetSecurityInfoAsync_WithInvalidUserId_ReturnsNull()
    {
        // Act
        var result = await _service.GetSecurityInfoAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetSessionsAsync Tests

    [Fact]
    public async Task GetSessionsAsync_ReturnsAtLeastOneSession()
    {
        // Act
        var result = await _service.GetSessionsAsync(_testUserId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, s => s.IsCurrent);
    }

    [Fact]
    public async Task GetSessionsAsync_WithInvalidUserId_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetSessionsAsync(Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region RevokeSessionAsync Tests

    [Fact]
    public async Task RevokeSessionAsync_WithValidSessionId_ReturnsTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();

        // Act
        var result = await _service.RevokeSessionAsync(_testUserId, sessionId);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region RevokeAllSessionsAsync Tests

    [Fact]
    public async Task RevokeAllSessionsAsync_ReturnsTrue()
    {
        // Act
        var result = await _service.RevokeAllSessionsAsync(_testUserId);

        // Assert
        Assert.True(result);
    }

    #endregion
}