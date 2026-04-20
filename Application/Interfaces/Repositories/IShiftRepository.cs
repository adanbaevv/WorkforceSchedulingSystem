using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IShiftRepository
    {
        Task<Shift?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Shift>> GetOpenShiftsAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Shift shift, CancellationToken cancellationToken = default);
        Task UpdateAsync(Shift shift, CancellationToken cancellationToken = default);
    }
}
