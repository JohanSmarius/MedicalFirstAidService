using System;
using System.Collections.Generic;
using System.Text;

namespace Event.API
{
    internal interface IStaffService
    {
        Task<StaffDTO> CreateStaffAsync(StaffDTO newStaff);
        Task<List<StaffDTO>> GetAllStaffAsync();
        Task<StaffDTO> GetStaffByIdAsync(int id);
        Task ResignStaffAsync(StaffDTO staff);
        Task<StaffDTO> UpdateStaffAsync(StaffDTO staff);
    }
}
