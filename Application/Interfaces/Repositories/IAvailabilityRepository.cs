using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IAvailabilityRepository
    {
        Task<Availability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Availability>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Availability>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task AddAsync(Availability availability, CancellationToken cancellationToken = default);
        Task UpdateAsync(Availability availability, CancellationToken cancellationToken = default);
    }
}
