using Event.Domain;

namespace Event.DomainServices;

/// <summary>
/// Service for managing staff
/// </summary>
internal interface IStaffRepository
{
    Task<List<Staff>> GetAllStaffAsync();
    Task<Staff?> GetStaffByIdAsync(int id);
    Task<Staff> CreateStaffAsync(Staff staff);
    Task<Staff> UpdateStaffAsync(Staff staff);
    Task DeleteStaffAsync(int id);
    Task<List<Staff>> GetActiveStaffAsync();
    Task<List<Staff>> GetStaffByRoleAsync(StaffRole role);
}
