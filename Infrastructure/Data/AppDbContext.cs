using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Shift> Shifts => Set<Shift>();
        public DbSet<ShiftRequest> ShiftRequests => Set<ShiftRequest>();
        public DbSet<Availability> Availabilities => Set<Availability>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShiftRequest>()
                .Property(request => request.Reason)
                .HasMaxLength(500)
                .IsRequired(false);

            modelBuilder.Entity<TimeEntry>()
                .HasOne(timeEntry => timeEntry.Employee)
                .WithMany()
                .HasForeignKey(timeEntry => timeEntry.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<TimeEntry>()
                .HasOne(timeEntry => timeEntry.Shift)
                .WithMany()
                .HasForeignKey(timeEntry => timeEntry.ShiftId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var modifiedEntries = ChangeTracker
                .Entries<BaseEntity>()
                .Where(entry => entry.State == EntityState.Modified);

            foreach (var entry in modifiedEntries)
            {
                entry.Property(entity => entity.UpdatedAt).CurrentValue = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
