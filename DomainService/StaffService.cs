using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using Microsoft.Extensions.Logging;

namespace DomainService
{
    public class StaffService : IStaffService
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
            staff.UpdatedAt = DateTime.UtcNow;
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

        public async Task<Staff> CreateStaffAsync(Staff newStaff)
        {
            // Validate email uniqueness
            var isEmailUnique = await _staffRepository.IsEmailUniqueAsync(newStaff.Email);
            if (!isEmailUnique)
            {
                throw new DomainException("Email address is already in use.");
            }

            newStaff.IsActive = true;
            newStaff.CreatedAt = DateTime.UtcNow;

            var created = await _staffRepository.CreateStaffAsync(newStaff);

            return created;
        }

        public async Task<Staff> GetStaffByIdAsync(int id)
        {
            return await _staffRepository.GetStaffByIdAsync(id) ??
                throw new InvalidOperationException($"Staff {id} not found");
        }

        public async Task<List<Staff>> GetAllStaffAsync()
        {
            return await _staffRepository.GetAllStaffAsync();
        }

        public async Task<Staff> UpdateStaffAsync(Staff updated)
        {
            // Validate email uniqueness
            var isEmailUnique = await _staffRepository.IsEmailUniqueAsync(updated.Email, updated.Id);
            if (!isEmailUnique)
            {
                throw new DomainException("Email address is already in use.");
            }

            var existing = await _staffRepository.GetStaffByIdAsync(updated.Id) ??
                throw new InvalidOperationException($"Staff {updated.Id} not found");

            // Update fields
            existing.FirstName = updated.FirstName;
            existing.LastName = updated.LastName;
            existing.Email = updated.Email;
            existing.Phone = updated.Phone;
            existing.Role = updated.Role;
            existing.CertificationLevel = updated.CertificationLevel;
            existing.CertificationExpiry = updated.CertificationExpiry;
            existing.UpdatedAt = DateTime.UtcNow;

            await _staffRepository.UpdateStaffAsync(existing);
            return existing;
        }
    }
}
