using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ShiftRepository : IShiftRepository
    {
        private readonly AppDbContext _context;

        public ShiftRepository(AppDbContext context)
        {
            _context = context;
        }

        public Shift GetById(Guid id)
            => _context.Shifts.Find(id);

        public IEnumerable<Shift> GetOpenShifts()
            => _context.Shifts
                .Where(s => s.Status == ShiftStatus.OpenForPickup)
                .ToList();

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
