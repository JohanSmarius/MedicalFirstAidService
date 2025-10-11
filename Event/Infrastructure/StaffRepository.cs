using Event.Domain;
using Event.DomainServices;
using Event.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure;

internal class StaffRepository : IStaffRepository
{
    private readonly EventDbContext _context;

    public StaffRepository(EventDbContext context)
    {
        _context = context;
    }

    public async Task<List<Domain.Staff>> GetAllStaffAsync()
    {
        return await _context.Staff
            .ToListAsync();
    }

    public async Task<Domain.Staff?> GetStaffByIdAsync(int id)
    {
        return await _context.Staff
            .Include(s => s.StaffAssignments)
            .ThenInclude(sa => sa.Shift)
            .ThenInclude(s => s.Event)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Domain.Staff> CreateStaffAsync(Staff staff)
    {
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();
        return staff;
    }

    public async Task<Domain.Staff> UpdateStaffAsync(Staff staff)
    {
        _context.Staff.Update(staff);
        await _context.SaveChangesAsync();
        return staff;
    }

    public async Task DeleteStaffAsync(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff != null)
        {
            // Soft delete by setting IsActive to false
            staff.IsActive = false;
            _context.Staff.Update(staff);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Domain.Staff>> GetActiveStaffAsync()
    {
        return await _context.Staff
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<List<Staff>> GetStaffByRoleAsync(StaffRole role)
    {
        return await _context.Staff
            .Where(s => s.Role == role && s.IsActive)
            .ToListAsync();
    }
}
