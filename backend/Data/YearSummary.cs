using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetFuel.Data;

/// <summary>
/// Represents a yearly summary that can be locked for tax compliance.
/// </summary>
public class YearSummary
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int Year { get; set; }

    public int TotalTrips { get; set; }

    public decimal TotalDistance { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalExpenses { get; set; }

    public bool IsLocked { get; set; } = false;

    public DateTime? LockedAt { get; set; }

    public Guid? LockedByUserId { get; set; }

    public string? LockReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
