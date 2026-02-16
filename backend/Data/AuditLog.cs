using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetFuel.Data;

/// <summary>
/// Immutable audit log entry for compliance and security tracking.
/// </summary>
public class AuditLog
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = null!;

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = null!; // Create, Update, Delete, Unlock

    public string? OldValues { get; set; } // JSON

    public string? NewValues { get; set; } // JSON

    [Required]
    public Guid UserId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Reason { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
