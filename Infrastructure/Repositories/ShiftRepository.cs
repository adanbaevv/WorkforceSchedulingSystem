using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ShiftRepository : IShiftRepository
    {
        private readonly AppDbContext _context;

        public ShiftRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Shift?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Shifts.FindAsync(new object[] { id }, cancellationToken);

        public async Task<IReadOnlyList<Shift>> GetOpenShiftsAsync(CancellationToken cancellationToken = default)
            => await _context.Shifts
                .Where(s => s.Status == ShiftStatus.OpenForPickup)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Shift shift, CancellationToken cancellationToken = default)
        {
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Shift shift, CancellationToken cancellationToken = default)
        {
            _context.Shifts.Update(shift);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
