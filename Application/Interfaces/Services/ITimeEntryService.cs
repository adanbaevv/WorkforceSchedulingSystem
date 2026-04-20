using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface ITimeEntryService
    {
        /// <summary>
        /// Clocks an employee in and creates a new active time entry.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="shiftId">The optional shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created time entry.</returns>
        Task<TimeEntry> ClockInAsync(Guid employeeId, Guid? shiftId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clocks a time entry out and stores optional notes.
        /// </summary>
        /// <param name="timeEntryId">The time entry identifier.</param>
        /// <param name="notes">Optional clock-out notes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated time entry.</returns>
        Task<TimeEntry> ClockOutAsync(Guid timeEntryId, string? notes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the active time entry for an employee, if one exists.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The active time entry, or <c>null</c>.</returns>
        Task<TimeEntry?> GetActiveByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the full time entry history for an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of time entries.</returns>
        Task<IReadOnlyList<TimeEntry>> GetHistoryByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    }
}
