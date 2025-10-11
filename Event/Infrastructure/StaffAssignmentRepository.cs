using Event.Domain;
using Event.DomainServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Event.Infrastructure.Data;

namespace Event.Infrastructure;

internal class StaffAssignmentRepository : IStaffAssignmentRepository
{
    private readonly EventDbContext _context;
    private readonly ILogger<StaffAssignmentRepository> _logger;

    public StaffAssignmentRepository(EventDbContext context, ILogger<StaffAssignmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<StaffAssignment>> GetAllAssignmentsAsync()
    {
        return await _context.StaffAssignments
            .Include(sa => sa.Staff)
            .Include(sa => sa.Shift)
            .ThenInclude(s => s.Event)
            .OrderBy(sa => sa.Shift.StartTime)
            .ToListAsync();
    }

    public async Task<StaffAssignment?> GetAssignmentByIdAsync(int id)
    {
        return await _context.StaffAssignments
            .Include(sa => sa.Staff)
            .Include(sa => sa.Shift)
            .ThenInclude(s => s.Event)
            .FirstOrDefaultAsync(sa => sa.Id == id);
    }

    public async Task<List<StaffAssignment>> GetAssignmentsByShiftIdAsync(int shiftId)
    {
        return await _context.StaffAssignments
            .Include(sa => sa.Staff)
            .Where(sa => sa.ShiftId == shiftId)
            .ToListAsync();
    }

    public async Task<List<StaffAssignment>> GetAssignmentsByStaffIdAsync(int staffId)
    {
        return await _context.StaffAssignments
            .Include(sa => sa.Shift)
            .ThenInclude(s => s.Event)
            .Where(sa => sa.StaffId == staffId)
            .OrderBy(sa => sa.Shift.StartTime)
            .ToListAsync();
    }

    public async Task<StaffAssignment> CreateAssignmentAsync(StaffAssignment assignment)
    {
        assignment.AssignedAt = DateTime.UtcNow;
        _context.StaffAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<StaffAssignment> UpdateAssignmentAsync(StaffAssignment assignment)
    {
        assignment.UpdatedAt = DateTime.UtcNow;
        _context.StaffAssignments.Update(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task DeleteAssignmentAsync(int id)
    {
        var assignment = await _context.StaffAssignments.FindAsync(id);
        if (assignment != null)
        {
            _context.StaffAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<StaffAssignment?> CheckInStaffAsync(int assignmentId)
    {
        var assignment = await GetAssignmentByIdAsync(assignmentId);
        if (assignment != null)
        {
            assignment.CheckInTime = DateTime.UtcNow;
            assignment.Status = AssignmentStatus.CheckedIn;
            await UpdateAssignmentAsync(assignment);
        }
        return assignment;
    }

    public async Task<StaffAssignment?> CheckOutStaffAsync(int assignmentId)
    {
        var assignment = await GetAssignmentByIdAsync(assignmentId);
        if (assignment != null)
        {
            assignment.CheckOutTime = DateTime.UtcNow;
            assignment.Status = AssignmentStatus.CheckedOut;
            await UpdateAssignmentAsync(assignment);
        }
        return assignment;
    }

    public async Task<bool> IsStaffAvailableAsync(int staffId, DateTime startTime, DateTime endTime, int? excludeAssignmentId = null)
    {
        var query = _context.StaffAssignments
            .Include(sa => sa.Shift)
            .Where(sa => sa.StaffId == staffId &&
                        sa.Status != AssignmentStatus.Cancelled &&
                        sa.Shift.StartTime < endTime &&
                        sa.Shift.EndTime > startTime);

        if (excludeAssignmentId.HasValue)
        {
            query = query.Where(sa => sa.Id != excludeAssignmentId.Value);
        }

        return !await query.AnyAsync();
    }
}
