using Event.Domain;
using Event.API;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.DomainServices;

internal class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IStaffAssignmentRepository _assignmentRepository;
    private readonly ILogger<StaffService> _logger;

    public StaffService(
        IStaffRepository staffRepository,
        IStaffAssignmentRepository assignmentRepository,
        ILogger<StaffService> logger)
    {
        _staffRepository = staffRepository;
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    public async Task ResignStaffAsync(Staff staff)
    {
        if (staff == null)
        {
            throw new ArgumentNullException(nameof(staff));
        }

        if (!staff.IsActive)
        {
            _logger.LogWarning("Staff member {StaffId} is already inactive.", staff.Id);
            return;
        }

        // Deactivate the staff member
        staff.IsActive = false;
        await _staffRepository.UpdateStaffAsync(staff);

        // Get all assignments for the staff
        var assignments = await _assignmentRepository.GetAssignmentsByStaffIdAsync(staff.Id);

        // Cancel all active assignments
        foreach (var assignment in assignments)
        {
            if (assignment.Status == AssignmentStatus.Assigned || assignment.Status == AssignmentStatus.Confirmed)
            {
                assignment.Status = AssignmentStatus.Cancelled;
                assignment.UpdatedAt = DateTime.UtcNow;
                await _assignmentRepository.UpdateAssignmentAsync(assignment);
                _logger.LogInformation("Cancelled assignment {AssignmentId} for resigning staff {StaffId}", assignment.Id, staff.Id);
            }
        }

        _logger.LogInformation("Staff member {StaffId} has resigned and all assignments have been cancelled.", staff.Id);
    }
}
