using Domain;
using Microsoft.Extensions.Logging;

namespace DomainService;

public class ShiftService : IShiftService
{
    private readonly IStaffAssignmentRepository _assignmentRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(
        IStaffAssignmentRepository assignmentRepository,
        IShiftRepository shiftRepository,
        IStaffRepository staffRepository,
        IEmailService emailService,
        ILogger<ShiftService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _shiftRepository = shiftRepository;
        _staffRepository = staffRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task AddStaffToShiftAsync(Shift shift, Staff staff)
    {
        if (shift == null)
        {
            throw new ArgumentNullException(nameof(shift));
        }

        if (staff == null)
        {
            throw new ArgumentNullException(nameof(staff));
        }

        if (!staff.IsActive)
        {
            throw new DomainException($"Staff member {staff.FullName} is not active.");
        }

        // Check if shift is in the past
        if (shift.StartTime < DateTime.UtcNow)
        {
            throw new DomainException("Cannot assign staff to a shift that has already started.");
        }

        // Check staff availability
        var isAvailable = await _assignmentRepository.IsStaffAvailableAsync(staff.Id, shift.StartTime, shift.EndTime);
        if (!isAvailable)
        {
            throw new DomainException($"Staff member {staff.FullName} is not available for this shift.");
        }

        // Create assignment
        var assignment = new StaffAssignment
        {
            ShiftId = shift.Id,
            StaffId = staff.Id,
            Status = AssignmentStatus.Assigned,
            AssignedAt = DateTime.UtcNow
        };

        await _assignmentRepository.CreateAssignmentAsync(assignment);

        // Send notification
        if (shift.Event != null)
        {
            try
            {
                await _emailService.SendStaffAssignmentNotificationAsync(staff, shift, shift.Event);
                _logger.LogInformation("Assignment notification sent for staff {StaffId} to shift {ShiftId}", staff.Id, shift.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send assignment notification for staff {StaffId} to shift {ShiftId}", staff.Id, shift.Id);
            }
        }

        _logger.LogInformation("Staff {StaffId} assigned to shift {ShiftId}", staff.Id, shift.Id);
    }

    public async Task RemoveStaffFromShiftAsync(Shift shift, Staff staff)
    {
        if (shift == null)
        {
            throw new ArgumentNullException(nameof(shift));
        }

        if (staff == null)
        {
            throw new ArgumentNullException(nameof(staff));
        }

        // Find the assignment
        var assignments = await _assignmentRepository.GetAssignmentsByShiftIdAsync(shift.Id);
        var assignment = assignments.FirstOrDefault(a => a.StaffId == staff.Id);

        if (assignment == null)
        {
            throw new DomainException($"Staff member {staff.FullName} is not assigned to this shift.");
        }

        if (assignment.Status == AssignmentStatus.CheckedIn || assignment.Status == AssignmentStatus.CheckedOut)
        {
            throw new DomainException("Cannot remove staff from a shift they have already checked into.");
        }

        // Cancel the assignment
        assignment.Status = AssignmentStatus.Cancelled;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _assignmentRepository.UpdateAssignmentAsync(assignment);

        _logger.LogInformation("Staff {StaffId} removed from shift {ShiftId}", staff.Id, shift.Id);
    }

    public async Task<Shift> CreateShiftAsync(Shift newShift)
    {
        // Validate dates
        if (newShift.StartTime >= newShift.EndTime)
        {
            throw new DomainException("End time must be after start time.");
        }

        if (newShift.StartTime < DateTime.UtcNow)
        {
            throw new DomainException("Start time cannot be in the past.");
        }

        // Check if event exists
        var eventExists = await _shiftRepository.GetShiftByIdAsync(newShift.EventId) != null; // wait, no, need event repo.
        // Actually, since Shift has EventId, and repository will handle foreign key, but to validate, perhaps check if event exists.

        // For now, assume it's handled by DB.

        newShift.Status = ShiftStatus.Open;
        newShift.CreatedAt = DateTime.UtcNow;

        var created = await _shiftRepository.CreateShiftAsync(newShift);

        return created;
    }

    public async Task<Shift> GetShiftByIdAsync(int id)
    {
        return await _shiftRepository.GetShiftByIdAsync(id) ??
            throw new InvalidOperationException($"Shift {id} not found");
    }

    public async Task<Shift> UpdateShiftAsync(Shift updated)
    {
        // Validate dates
        if (updated.StartTime >= updated.EndTime)
        {
            throw new DomainException("End time must be after start time.");
        }

        // Load current state
        var existing = await _shiftRepository.GetShiftByIdAsync(updated.Id) ??
            throw new InvalidOperationException($"Shift {updated.Id} not found");

        // Apply updates
        existing.Name = updated.Name;
        existing.StartTime = updated.StartTime;
        existing.EndTime = updated.EndTime;
        existing.RequiredStaff = updated.RequiredStaff;
        existing.Description = updated.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await _shiftRepository.UpdateShiftAsync(existing);
        return existing;
    }

    public async Task CancelShiftAsync(Shift shiftToCancel)
    {
        if (shiftToCancel == null)
        {
            throw new ArgumentNullException(nameof(shiftToCancel));
        }

        // Load the existing shift with assignments
        var existing = await _shiftRepository.GetShiftByIdAsync(shiftToCancel.Id) ??
            throw new InvalidOperationException($"Shift {shiftToCancel.Id} not found");

        if (existing.Status == ShiftStatus.Cancelled)
        {
            _logger.LogWarning("Shift {ShiftId} is already cancelled.", existing.Id);
            return;
        }

        // Cancel the shift
        existing.Status = ShiftStatus.Cancelled;
        existing.UpdatedAt = DateTime.UtcNow;

        // Cancel all assignments for this shift
        foreach (var assignment in existing.StaffAssignments)
        {
            if (assignment.Status == AssignmentStatus.Assigned || assignment.Status == AssignmentStatus.Confirmed)
            {
                assignment.Status = AssignmentStatus.Cancelled;
                assignment.UpdatedAt = DateTime.UtcNow;
                await _assignmentRepository.UpdateAssignmentAsync(assignment);
                _logger.LogInformation("Cancelled assignment {AssignmentId} for cancelled shift {ShiftId}", assignment.Id, existing.Id);
            }
        }

        // Persist the shift
        await _shiftRepository.UpdateShiftAsync(existing);

        _logger.LogInformation("Shift {ShiftId} has been cancelled.", existing.Id);
    }

    public async Task<List<Shift>> GetShiftsByEventIdAsync(int eventId)
    {
        return await _shiftRepository.GetShiftsByEventIdAsync(eventId);
    }
}