using System;
using System.Collections.Generic;
using System.Text;

namespace Event.API
{
    internal interface IShiftService
    {
        Task AddStaffToShiftAsync(ShiftDTO shift, StaffDTO staff);
        Task CancelShiftAsync(ShiftDTO shiftToCancel);
        Task<ShiftDTO> CreateShiftAsync(ShiftDTO newShift);
        Task<ShiftDTO> GetShiftByIdAsync(int id);
        Task<List<ShiftDTO>> GetShiftsByEventIdAsync(int eventId);
        Task RemoveStaffFromShiftAsync(ShiftDTO shift, StaffDTO staff);
        Task<ShiftDTO> UpdateShiftAsync(ShiftDTO updated);
    }
}
