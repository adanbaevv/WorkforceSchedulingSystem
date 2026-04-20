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
    public class ShiftRequestRepository : IShiftRequestRepository
    {
        private readonly AppDbContext _context;

        public ShiftRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public ShiftRequest GetById(Guid id)
            => _context.ShiftRequests.Find(id);

        public IEnumerable<ShiftRequest> GetPending()
            => _context.ShiftRequests
                .Where(r => r.Status == ShiftRequestStatus.Pending)
                .ToList();

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
