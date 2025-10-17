using Microsoft.EntityFrameworkCore;
using domain = Staff.Domain;

namespace Infrastructure.Data;

internal class StaffDbContext(DbContextOptions<StaffDbContext> options) 
    : DbContext(options)
{
    public DbSet<domain.Staff> Staff { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("Staff");

        builder.Entity<domain.Staff>()
            .HasIndex(s => s.Email)
            .IsUnique();
    }
}
