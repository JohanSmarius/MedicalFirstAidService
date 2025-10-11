using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ds = Event.DomainServices;

namespace Event.API
{
    internal class ShiftService : IShiftService
    {
        private readonly ds.IShiftService internalShiftService;

        public ShiftService(ds.IShiftService internalShiftService)
        {
            this.internalShiftService = internalShiftService;
        }

        public async Task<ShiftDTO> CreateShiftAsync(ShiftDTO newShift)
        {
            var domainShift = ToDomain(newShift);
            var created = await internalShiftService.CreateShiftAsync(domainShift);
            return ToDTO(created);
        }

        public async Task<ShiftDTO> GetShiftByIdAsync(int id)
        {
            var domainShift = await internalShiftService.GetShiftByIdAsync(id);
            return ToDTO(domainShift);
        }

        public async Task<List<ShiftDTO>> GetShiftsByEventIdAsync(int eventId)
        {
            var domainShifts = await internalShiftService.GetShiftsByEventIdAsync(eventId);
            return domainShifts.Select(ToDTO).ToList();
        }

        public async Task<ShiftDTO> UpdateShiftAsync(ShiftDTO updated)
        {
            var domainShift = ToDomain(updated);
            var updatedDomain = await internalShiftService.UpdateShiftAsync(domainShift);
            return ToDTO(updatedDomain);
        }

        public async Task CancelShiftAsync(ShiftDTO shiftToCancel)
        {
            var domainShift = ToDomain(shiftToCancel);
            await internalShiftService.CancelShiftAsync(domainShift);
        }

        public async Task AddStaffToShiftAsync(ShiftDTO shift, StaffDTO staff)
        {
            var domainShift = ToDomain(shift);
            var domainStaff = ToDomain(staff);
            await internalShiftService.AddStaffToShiftAsync(domainShift, domainStaff);
        }

        public async Task RemoveStaffFromShiftAsync(ShiftDTO shift, StaffDTO staff)
        {
            var domainShift = ToDomain(shift);
            var domainStaff = ToDomain(staff);
            await internalShiftService.RemoveStaffFromShiftAsync(domainShift, domainStaff);
        }

        private static Event.Domain.Shift ToDomain(ShiftDTO dto)
        {
            return new Event.Domain.Shift
            {
                Id = dto.Id,
                EventId = dto.EventId,
                Name = dto.Name,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                RequiredStaff = dto.RequiredStaff,
                Description = dto.Description,
                Status = (Event.Domain.ShiftStatus)dto.Status
            };
        }

        private static ShiftDTO ToDTO(Event.Domain.Shift domain)
        {
            return new ShiftDTO
            {
                Id = domain.Id,
                EventId = domain.EventId,
                Name = domain.Name,
                StartTime = domain.StartTime,
                EndTime = domain.EndTime,
                RequiredStaff = domain.RequiredStaff,
                Description = domain.Description,
                Status = (ShiftStatusDTO)domain.Status
            };
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
    }
}
