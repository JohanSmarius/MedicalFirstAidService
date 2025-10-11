using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Event.Domain;

/// <summary>
/// Represents a staff member
/// </summary>
internal class Staff
{
    public int Id { get; set; }

    [Required]
    public int ReferenceId { get; set; } // Reference to Domain.Staff

    [Required]
    public StaffRole Role { get; set; }

    // Navigation properties
    public List<StaffAssignment> StaffAssignments { get; set; } = new();

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Role of a staff member
/// </summary>
public enum StaffRole
{
    FirstAider,
    TeamLeader,
    Paramedic,
    Doctor,
    Volunteer
}
