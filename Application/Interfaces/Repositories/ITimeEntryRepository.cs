using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITimeEntryRepository
    {
        Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TimeEntry?> GetActiveByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TimeEntry>> GetHistoryByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
        Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
    }
}
