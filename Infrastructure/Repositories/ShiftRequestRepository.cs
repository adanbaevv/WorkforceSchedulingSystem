using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ShiftRequestRepository : IShiftRequestRepository
    {
        private readonly AppDbContext _context;

        public ShiftRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ShiftRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.ShiftRequests.FindAsync(new object[] { id }, cancellationToken);

        public async Task<IReadOnlyList<ShiftRequest>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.ShiftRequests
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<ShiftRequest>> GetPendingAsync(CancellationToken cancellationToken = default)
            => await _context.ShiftRequests
                .Where(r => r.Status == ShiftRequestStatus.Pending)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<ShiftRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => await _context.ShiftRequests
                .Where(r => r.RequestedByEmployeeId == employeeId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(ShiftRequest request, CancellationToken cancellationToken = default)
        {
            _context.ShiftRequests.Add(request);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ShiftRequest request, CancellationToken cancellationToken = default)
        {
            _context.ShiftRequests.Update(request);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
