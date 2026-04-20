using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IShiftRequestRepository
    {
        Task<ShiftRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ShiftRequest>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ShiftRequest>> GetPendingAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ShiftRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task AddAsync(ShiftRequest request, CancellationToken cancellationToken = default);
        Task UpdateAsync(ShiftRequest request, CancellationToken cancellationToken = default);
    }
}
