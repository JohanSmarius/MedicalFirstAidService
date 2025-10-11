using System.ComponentModel.DataAnnotations;

namespace Event.Domain;

/// <summary>
/// Represents assignment of staff to a shift
/// </summary>
public class StaffAssignmentDTO
{
    public int Id { get; set; }

    [Required]
    public int ShiftId { get; set; }

    [Required]
    public int StaffId { get; set; }

    public AssignmentStatusDTO Status { get; set; } = AssignmentStatusDTO.Assigned;
}

/// <summary>
/// Status of a staff assignment
/// </summary>
public enum AssignmentStatusDTO
{
    Assigned,
    Confirmed,
    CheckedIn,
    CheckedOut,
    NoShow,
    Cancelled
}
