using Event.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ds = Event.DomainServices;

namespace Event.API;

internal class StaffService : IStaffService
{
    private readonly ds.IStaffRepository staffRepository;

    public StaffService(ds.IStaffRepository staffRepository)
    {
        this.staffRepository = staffRepository;
    }

    public async Task<StaffDTO> CreateStaffAsync(StaffDTO newStaff)
    {
        var domainStaff = ToDomain(newStaff);
        var created = await staffRepository.CreateStaffAsync(domainStaff);
        return ToDTO(created);
    }

    public async Task<List<StaffDTO>> GetAllStaffAsync()
    {
        var domainStaffList = await staffRepository.GetAllStaffAsync();
        return domainStaffList.Select(ToDTO).ToList();
    }

    public async Task<StaffDTO> GetStaffByIdAsync(int id)
    {
        var domainStaff = await staffRepository.GetStaffByIdAsync(id);
        if (domainStaff == null) throw new InvalidOperationException($"Staff {id} not found");
        return ToDTO(domainStaff);
    }

    public async Task<StaffDTO> UpdateStaffAsync(StaffDTO staff)
    {
        var domainStaff = ToDomain(staff);
        var updated = await staffRepository.UpdateStaffAsync(domainStaff);
        return ToDTO(updated);
    }

    public async Task ResignStaffAsync(StaffDTO staff)
    {
        var domainStaff = ToDomain(staff);
        await staffRepository.DeleteStaffAsync(domainStaff.Id); // Assuming soft delete
    }

    private static Event.Domain.Staff ToDomain(StaffDTO dto)
    {
        return new Event.Domain.Staff
        {
            Id = dto.Id,
            ReferenceId = dto.ReferenceId,
            Role = (Event.Domain.StaffRole)dto.Role,
            IsActive = dto.IsActive
        };
    }

    private static StaffDTO ToDTO(Event.Domain.Staff domain)
    {
        return new StaffDTO
        {
            Id = domain.Id,
            ReferenceId = domain.ReferenceId,
            Role = (StaffRoleDTO)domain.Role,
            IsActive = domain.IsActive
        };
    }
}
