using System;
using System.Collections.Generic;
using System.Text;

namespace Staff.API
{
    public interface IStaffService
    {
        Task<StaffDTO> CreateStaffAsync(StaffDTO newStaff);
        Task<StaffDTO> GetStaffByIdAsync(int id);
        Task<List<StaffDTO>> GetAllStaffAsync();
        Task<StaffDTO> UpdateStaffAsync(StaffDTO staff);
        Task ResignStaffAsync(StaffDTO staff);
    }
}
