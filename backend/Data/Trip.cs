using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetFuel.Data;

/// <summary>
/// Trip entity - represents a logged trip (Fahrtenbuch-style).
/// Kilometers are calculated as EndKm - StartKm.
/// </summary>
public class Trip
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid VehicleId { get; set; }

    public DateTime Date { get; set; }

    [Required]
    public int StartKm { get; set; }

    [Required]
    public int EndKm { get; set; }

    [NotMapped]
    public int CalculatedKm => EndKm - StartKm;

    [MaxLength(500)]
    public string? Purpose { get; set; }

    public bool IsBusiness { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;
}