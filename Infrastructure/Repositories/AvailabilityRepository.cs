using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly AppDbContext _context;

        public AvailabilityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Availability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Availabilities.FindAsync(new object[] { id }, cancellationToken);

        public async Task<IReadOnlyList<Availability>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Availabilities
                .OrderBy(availability => availability.EmployeeId)
                .ThenBy(availability => availability.DayOfWeek)
                .ThenBy(availability => availability.StartTime)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<Availability>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => await _context.Availabilities
                .Where(availability => availability.EmployeeId == employeeId)
                .OrderBy(availability => availability.DayOfWeek)
                .ThenBy(availability => availability.StartTime)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Availability availability, CancellationToken cancellationToken = default)
        {
            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Availability availability, CancellationToken cancellationToken = default)
        {
            _context.Availabilities.Update(availability);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Availability availability, CancellationToken cancellationToken = default)
        {
            _context.Availabilities.Remove(availability);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
