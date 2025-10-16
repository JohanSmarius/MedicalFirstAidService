using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using Microsoft.Extensions.Logging;
using StaffEvents;
using domain = Staff.Domain;

namespace Staff.DomainServices;

internal class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger<StaffService> _logger;
    private readonly IBus _bus;

    public StaffService(
        IStaffRepository staffRepository,
        ILogger<StaffService> logger,
        IBus bus)
    {
        _staffRepository = staffRepository;
        _logger = logger;
        _bus = bus;
    }

    public async Task ResignStaffAsync(domain.Staff staff)
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

       
        var message = new StaffResignedEvent() { Id = staff.Id };
        await _bus.Publish(message);

        _logger.LogInformation("Staff member {StaffId} has resigned and all assignments have been cancelled.", staff.Id);
    }

    public async Task<domain.Staff> CreateStaffAsync(domain.Staff newStaff)
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

        var message = new StafCreatedEvent() { Id = created.Id };
        await _bus.Publish(message);

        return created;
    }

    public async Task<domain.Staff> GetStaffByIdAsync(int id)
    {
        return await _staffRepository.GetStaffByIdAsync(id) ??
            throw new InvalidOperationException($"Staff {id} not found");
    }

    public async Task<List<domain.Staff>> GetAllStaffAsync()
    {
        return await _staffRepository.GetAllStaffAsync();
    }

    public async Task<domain.Staff> UpdateStaffAsync(domain.Staff updated)
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
