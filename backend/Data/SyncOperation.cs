using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetFuel.Data;

/// <summary>
/// Represents a pending sync operation for offline-first support.
/// </summary>
public class SyncOperation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = null!; // Vehicle, Trip, Receipt

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public int OperationType { get; set; } // 1=Create, 2=Update, 3=Delete

    public string? Payload { get; set; } // JSON

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int RetryCount { get; set; }

    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}