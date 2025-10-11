using domain = Event.Domain;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.Data;

internal class EventDbContext(DbContextOptions<EventDbContext> options) 
    : DbContext(options)
{
    public DbSet<domain.Event> Events { get; set; } = null!;
    public DbSet<domain.Shift> Shifts { get; set; } = null!;
    public DbSet<domain.Staff> Staff { get; set; } = null!;
    public DbSet<domain.StaffAssignment> StaffAssignments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("Event");

        // Configure Event entity
        builder.Entity<domain.Event>()
            .HasMany(e => e.Shifts)
            .WithOne(s => s.Event)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Shift entity
        builder.Entity<domain.Shift>()
            .HasMany(s => s.StaffAssignments)
            .WithOne(sa => sa.Shift)
            .HasForeignKey(sa => sa.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Staff entity
        builder.Entity<domain.Staff>()
            .HasMany(s => s.StaffAssignments)
            .WithOne(sa => sa.Staff)
            .HasForeignKey(sa => sa.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for better performance
        builder.Entity<domain.Event>()
            .HasIndex(e => e.StartDate);

        builder.Entity<domain.Shift>()
            .HasIndex(s => s.StartTime);

        builder.Entity<domain.Staff>()
            .HasIndex(s => s.ReferenceId)
            .IsUnique();
    }
}
