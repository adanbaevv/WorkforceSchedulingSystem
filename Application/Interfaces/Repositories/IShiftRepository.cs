using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IShiftRepository
    {
        Task<IReadOnlyList<Shift>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Shift?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Shift>> GetOpenShiftsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Shift>> GetByDateRangeAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Shift>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task AddAsync(Shift shift, CancellationToken cancellationToken = default);
        Task UpdateAsync(Shift shift, CancellationToken cancellationToken = default);
    }
}
