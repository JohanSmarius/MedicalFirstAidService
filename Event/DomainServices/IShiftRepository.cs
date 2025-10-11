using domain = Event.Domain;

namespace Event.DomainServices;

/// <summary>
/// Service for managing shifts
/// </summary>
internal interface IShiftRepository
{
    Task<List<domain.Shift>> GetAllShiftsAsync();
    Task<domain.Shift?> GetShiftByIdAsync(int id);
    Task<List<domain.Shift>> GetShiftsByEventIdAsync(int eventId);
    Task<domain.Shift> CreateShiftAsync(domain.Shift shift);
    Task<domain.Shift> UpdateShiftAsync(domain.Shift shift);
    Task DeleteShiftAsync(int id);
    Task<List<domain.Shift>> GetUpcomingShiftsAsync();
    Task<List<domain.Shift>> GetShiftsByDateAsync(DateTime date);
}
