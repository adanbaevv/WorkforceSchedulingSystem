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
            => await (from request in _context.ShiftRequests
                      join employee in _context.Employees
                          on request.RequestedByEmployeeId equals employee.Id
                      where request.Id == id
                            && request.IsActive
                            && employee.IsActive
                      select request)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyList<ShiftRequest>> GetAllAsync(CancellationToken cancellationToken = default)
            => await (from request in _context.ShiftRequests
                      join employee in _context.Employees
                          on request.RequestedByEmployeeId equals employee.Id
                      where request.IsActive
                            && employee.IsActive
                      orderby request.RequestedAt descending
                      select request)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<ShiftRequest>> GetPendingAsync(CancellationToken cancellationToken = default)
            => await (from request in _context.ShiftRequests
                      join employee in _context.Employees
                          on request.RequestedByEmployeeId equals employee.Id
                      where request.IsActive
                            && employee.IsActive
                            && request.Status == ShiftRequestStatus.Pending
                      orderby request.RequestedAt descending
                      select request)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<ShiftRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => await (from request in _context.ShiftRequests
                      join employee in _context.Employees
                          on request.RequestedByEmployeeId equals employee.Id
                      where request.IsActive
                            && employee.IsActive
                            && request.RequestedByEmployeeId == employeeId
                      orderby request.RequestedAt descending
                      select request)
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
