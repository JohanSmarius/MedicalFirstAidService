using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace DomainService
{
    public interface IStaffService
    {
        Task<Staff> CreateStaffAsync(Staff newStaff);
        Task<Staff> GetStaffByIdAsync(int id);
        Task<List<Staff>> GetAllStaffAsync();
        Task<Staff> UpdateStaffAsync(Staff staff);
        Task ResignStaffAsync(Staff staff);
    }
}
