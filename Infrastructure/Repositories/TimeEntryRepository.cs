using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly AppDbContext _context;

        public TimeEntryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.TimeEntries.FindAsync(new object[] { id }, cancellationToken);

        public async Task<TimeEntry?> GetActiveByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => await _context.TimeEntries
                .Where(timeEntry => timeEntry.EmployeeId == employeeId && timeEntry.ClockOutAt == null)
                .OrderByDescending(timeEntry => timeEntry.ClockInAt)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyList<TimeEntry>> GetHistoryByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => await _context.TimeEntries
                .Where(timeEntry => timeEntry.EmployeeId == employeeId)
                .OrderByDescending(timeEntry => timeEntry.ClockInAt)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
        {
            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
        {
            _context.TimeEntries.Update(timeEntry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
