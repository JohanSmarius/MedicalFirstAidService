using System.Threading.Tasks;
using Event.Domain;

namespace Event.DomainServices;

internal interface IShiftService
{
    Task<Shift> CreateShiftAsync(Shift newShift);
    Task<Shift> GetShiftByIdAsync(int id);
    Task<List<Shift>> GetShiftsByEventIdAsync(int eventId);
    Task<Shift> UpdateShiftAsync(Shift shift);
    Task CancelShiftAsync(Shift shiftToCancel);
    Task AddStaffToShiftAsync(Shift shift, Staff staff);
    Task RemoveStaffFromShiftAsync(Shift shift, Staff staff);
}