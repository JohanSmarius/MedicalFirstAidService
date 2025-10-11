using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Staff.API
{
    internal class StaffService : IStaffService
    {
        private readonly DomainServices.IStaffService internalStaffService;

        public StaffService(DomainServices.IStaffService internalStaffService)
        {
            this.internalStaffService = internalStaffService;
        }

        public async Task<StaffDTO> CreateStaffAsync(StaffDTO newStaff)
        {
            var domainStaff = ToDomain(newStaff);
            var created = await internalStaffService.CreateStaffAsync(domainStaff);
            return ToDTO(created);
        }

        public async Task<List<StaffDTO>> GetAllStaffAsync()
        {
            var domainStaffList = await internalStaffService.GetAllStaffAsync();
            return domainStaffList.Select(ToDTO).ToList();
        }

        public async Task<StaffDTO> GetStaffByIdAsync(int id)
        {
            var domainStaff = await internalStaffService.GetStaffByIdAsync(id);
            return ToDTO(domainStaff);
        }

        public async Task ResignStaffAsync(StaffDTO staff)
        {
            var domainStaff = ToDomain(staff);
            await internalStaffService.ResignStaffAsync(domainStaff);
        }

        public async Task<StaffDTO> UpdateStaffAsync(StaffDTO staff)
        {
            var domainStaff = ToDomain(staff);
            var updated = await internalStaffService.UpdateStaffAsync(domainStaff);
            return ToDTO(updated);
        }

        private static Domain.Staff ToDomain(StaffDTO dto)
        {
            return new Domain.Staff
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = (Domain.StaffRole)dto.Role,
                CertificationLevel = dto.CertificationLevel,
                CertificationExpiry = dto.CertificationExpiry,
                IsActive = dto.IsActive
            };
        }

        private static StaffDTO ToDTO(Domain.Staff s)
        {
            return new StaffDTO
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                Phone = s.Phone,
                Role = (Staff.API.StaffRole)s.Role,
                CertificationLevel = s.CertificationLevel,
                CertificationExpiry = s.CertificationExpiry,
                IsActive = s.IsActive
            };
        }
    }
}
