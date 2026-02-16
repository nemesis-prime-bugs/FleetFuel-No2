using FleetFuel.Api.Services;
using FleetFuel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FleetFuel.Api.Tests.Services;

/// <summary>
/// Unit tests for SyncService following AAA pattern.
/// </summary>
public class SyncServiceTests
{
    private readonly FleetFuelDbContext _context;
    private readonly SyncService _service;
    private readonly Mock<ILogger<SyncService>> _loggerMock;

    public SyncServiceTests()
    {
        var options = new DbContextOptionsBuilder<FleetFuelDbContext>()
            .UseInMemoryDatabase(databaseName: $"SyncTestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new FleetFuelDbContext(options);
        _loggerMock = new Mock<ILogger<SyncService>>();
        _service = new SyncService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task QueueSyncOperationAsync_CreatesOperation()
    {
        // Arrange
        var operation = new SyncOperation
        {
            UserId = Guid.NewGuid(),
            EntityType = "Vehicle",
            EntityId = Guid.NewGuid(),
            OperationType = SyncOperationType.Create,
            Payload = "{\"name\":\"Test\"}"
        };

        // Act
        await _service.QueueSyncOperationAsync(operation);

        // Assert
        var operations = await _context.SyncOperations.ToListAsync();
        Assert.Single(operations);
        Assert.Equal("Vehicle", operations[0].EntityType);
        Assert.Equal(SyncOperationType.Create, operations[0].OperationType);
        Assert.Equal(0, operations[0].RetryCount);
        Assert.Null(operations[0].ProcessedAt);
    }

    [Fact]
    public async Task GetPendingOperationsAsync_ReturnsOnlyPending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var pendingOp = new SyncOperation
        {
            UserId = userId,
            EntityType = "Vehicle",
            EntityId = Guid.NewGuid(),
            OperationType = SyncOperationType.Create
        };
        
        var processedOp = new SyncOperation
        {
            UserId = userId,
            EntityType = "Trip",
            EntityId = Guid.NewGuid(),
            OperationType = SyncOperationType.Update,
            ProcessedAt = DateTime.UtcNow
        };

        _context.SyncOperations.AddRange(pendingOp, processedOp);
        await _context.SaveChangesAsync();

        // Act
        var pending = await _service.GetPendingOperationsAsync(userId);

        // Assert
        Assert.Single(pending);
        Assert.Equal("Vehicle", pending.First().EntityType);
    }

    [Fact]
    public async Task GetPendingOperationsAsync_ReturnsOrderedByCreatedAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var op1 = new SyncOperation { UserId = userId, EntityType = "A", EntityId = Guid.NewGuid(), OperationType = SyncOperationType.Create, CreatedAt = DateTime.UtcNow.AddMinutes(1) };
        var op2 = new SyncOperation { UserId = userId, EntityType = "B", EntityId = Guid.NewGuid(), OperationType = SyncOperationType.Create, CreatedAt = DateTime.UtcNow };
        
        _context.SyncOperations.AddRange(op2, op1);
        await _context.SaveChangesAsync();

        // Act
        var pending = (await _service.GetPendingOperationsAsync(userId)).ToList();

        // Assert
        Assert.Equal(2, pending.Count);
        Assert.True(pending[0].CreatedAt <= pending[1].CreatedAt);
    }

    [Fact]
    public async Task GetPendingOperationsAsync_LimitsResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        for (int i = 0; i < 150; i++)
        {
            _context.SyncOperations.Add(new SyncOperation
            {
                UserId = userId,
                EntityType = $"Entity{i}",
                EntityId = Guid.NewGuid(),
                OperationType = SyncOperationType.Create
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var pending = await _service.GetPendingOperationsAsync(userId);

        // Assert
        Assert.Equal(100, pending.Count());
    }

    [Fact]
    public async Task ProcessPendingOperationsAsync_ProcessesOperations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var operation = new SyncOperation
        {
            UserId = userId,
            EntityType = "Vehicle",
            EntityId = Guid.NewGuid(),
            OperationType = SyncOperationType.Create,
            Payload = "{}"
        };
        _context.SyncOperations.Add(operation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ProcessPendingOperationsAsync(userId);

        // Assert
        Assert.True(result);
        var processedOp = await _context.SyncOperations.FindAsync(operation.Id);
        Assert.NotNull(processedOp.ProcessedAt);
    }

    [Fact]
    public async Task ProcessPendingOperationsAsync_WithNoPending_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.ProcessPendingOperationsAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DetectConflictAsync_WhenNoConflict_ReturnsNoConflict()
    {
        // Arrange
        var entityType = "Vehicle";
        var entityId = Guid.NewGuid();
        var clientTimestamp = DateTime.UtcNow;

        // Act
        var resolution = await _service.DetectConflictAsync(entityType, entityId, clientTimestamp);

        // Assert
        Assert.False(resolution.ConflictDetected);
        Assert.Equal("NoConflict", resolution.Resolution);
    }

    [Fact]
    public async Task ResolveConflictAsync_WhenExists_MarksProcessed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var operation = new SyncOperation
        {
            UserId = userId,
            EntityType = "Vehicle",
            EntityId = Guid.NewGuid(),
            OperationType = SyncOperationType.Update
        };
        _context.SyncOperations.Add(operation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ResolveConflictAsync(operation.Id, "ServerWins");

        // Assert
        Assert.True(result);
        var resolved = await _context.SyncOperations.FindAsync(operation.Id);
        Assert.NotNull(resolved!.ProcessedAt);
    }

    [Fact]
    public async Task ResolveConflictAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        var fakeId = Guid.NewGuid();

        // Act
        var result = await _service.ResolveConflictAsync(fakeId, "ClientWins");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task QueueSyncOperationAsync_SetsRetryCount()
    {
        // Arrange
        var operation = new SyncOperation
        {
            UserId = Guid.NewGuid(),
            EntityType = "Trip",
            EntityId = Guid.NewGuid(),
            OperationType = SyncOperationType.Delete
        };

        // Act
        await _service.QueueSyncOperationAsync(operation);

        // Assert
        var queued = await _context.SyncOperations.FirstAsync();
        Assert.Equal(0, queued.RetryCount);
    }
}