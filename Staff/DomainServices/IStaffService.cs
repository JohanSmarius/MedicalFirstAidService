using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using domain = Staff.Domain;

namespace Staff.DomainServices;

internal interface IStaffService
{
    Task<domain.Staff> CreateStaffAsync(domain.Staff newStaff);
    Task<domain.Staff> GetStaffByIdAsync(int id);
    Task<List<domain.Staff>> GetAllStaffAsync();
    Task<domain.Staff> UpdateStaffAsync(domain.Staff staff);
    Task ResignStaffAsync(domain.Staff staff);
}
