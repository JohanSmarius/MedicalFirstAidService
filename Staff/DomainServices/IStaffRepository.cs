using domain = Staff.Domain;

namespace Staff.DomainServices;

/// <summary>
/// Service for managing staff
/// </summary>
internal interface IStaffRepository
{
    Task<List<domain.Staff>> GetAllStaffAsync();
    Task<domain.Staff?> GetStaffByIdAsync(int id);
    Task<domain.Staff> CreateStaffAsync(domain.Staff staff);
    Task<domain.Staff> UpdateStaffAsync(domain.Staff staff);
    Task DeleteStaffAsync(int id);
    Task<List<domain.Staff>> GetActiveStaffAsync();
    Task<List<domain.Staff>> GetStaffByRoleAsync(domain.StaffRole role);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
}
