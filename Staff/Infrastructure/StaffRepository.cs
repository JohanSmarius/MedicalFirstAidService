using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Staff.DomainServices;
using domain = Staff.Domain;

namespace Staff.Infrastructure;

internal class StaffRepository(StaffDbContext context) : IStaffRepository
{
    public async Task<List<Domain.Staff>> GetAllStaffAsync()
    {
        return await context.Staff
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<Domain.Staff?> GetStaffByIdAsync(int id)
    {
        return await context.Staff
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Domain.Staff> CreateStaffAsync(domain.Staff staff)
    {
        staff.CreatedAt = DateTime.UtcNow;
        context.Staff.Add(staff);
        await context.SaveChangesAsync();
        return staff;
    }

    public async Task<Domain.Staff> UpdateStaffAsync(domain.Staff staff)
    {
        staff.UpdatedAt = DateTime.UtcNow;
        context.Staff.Update(staff);
        await context.SaveChangesAsync();
        return staff;
    }

    public async Task DeleteStaffAsync(int id)
    {
        var staff = await context.Staff.FindAsync(id);
        if (staff != null)
        {
            // Soft delete by setting IsActive to false
            staff.IsActive = false;
            staff.UpdatedAt = DateTime.UtcNow;
            context.Staff.Update(staff);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Domain.Staff>> GetActiveStaffAsync()
    {
        return await context.Staff
            .Where(s => s.IsActive)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<List<domain.Staff>> GetStaffByRoleAsync(domain.StaffRole role)
    {
        return await context.Staff
            .Where(s => s.Role == role && s.IsActive)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var query = context.Staff.Where(s => s.Email == email);
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
