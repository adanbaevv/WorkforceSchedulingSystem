using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IShiftRepository _shiftRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;

        public TimeEntryService(
            ITimeEntryRepository timeEntryRepository,
            IEmployeeRepository employeeRepository,
            IShiftRepository shiftRepository)
        {
            _timeEntryRepository = timeEntryRepository;
            _employeeRepository = employeeRepository;
            _shiftRepository = shiftRepository;
        }

        /// <summary>
        /// Clocks an employee in and creates a new active time entry.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="shiftId">The optional shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created time entry.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the employee or shift does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when the employee already has an active time entry.</exception>
        public async Task<TimeEntry> ClockInAsync(Guid employeeId, Guid? shiftId, CancellationToken cancellationToken = default)
        {
            if (employeeId == Guid.Empty)
            {
                throw new ValidationException("EmployeeId is required.");
            }

            var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
            if (employee == null)
            {
                throw new NotFoundException($"Employee with id '{employeeId}' was not found.");
            }

            if (shiftId.HasValue)
            {
                var shift = await _shiftRepository.GetByIdAsync(shiftId.Value, cancellationToken);
                if (shift == null)
                {
                    throw new NotFoundException($"Shift with id '{shiftId.Value}' was not found.");
                }
            }

            var activeEntry = await _timeEntryRepository.GetActiveByEmployeeAsync(employeeId, cancellationToken);
            if (activeEntry != null)
            {
                throw new ConflictException("The employee already has an active time entry.");
            }

            var timeEntry = new TimeEntry(employeeId, shiftId, DateTime.UtcNow);
            await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);

            return timeEntry;
        }

        /// <summary>
        /// Clocks a time entry out and stores optional notes.
        /// </summary>
        /// <param name="timeEntryId">The time entry identifier.</param>
        /// <param name="notes">Optional clock-out notes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated time entry.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the time entry does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when the time entry is already clocked out.</exception>
        public async Task<TimeEntry> ClockOutAsync(Guid timeEntryId, string? notes, CancellationToken cancellationToken = default)
        {
            if (timeEntryId == Guid.Empty)
            {
                throw new ValidationException("Time entry id is required.");
            }

            var timeEntry = await _timeEntryRepository.GetByIdAsync(timeEntryId, cancellationToken);
            if (timeEntry == null)
            {
                throw new NotFoundException($"Time entry with id '{timeEntryId}' was not found.");
            }

            if (timeEntry.ClockOutAt.HasValue)
            {
                throw new ConflictException("The time entry is already clocked out.");
            }

            timeEntry.ClockOut(DateTime.UtcNow, notes);
            await _timeEntryRepository.UpdateAsync(timeEntry, cancellationToken);

            return timeEntry;
        }

        /// <summary>
        /// Retrieves the active time entry for an employee, if one exists.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The active time entry, or <c>null</c>.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        public async Task<TimeEntry?> GetActiveByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            if (employeeId == Guid.Empty)
            {
                throw new ValidationException("EmployeeId is required.");
            }

            return await _timeEntryRepository.GetActiveByEmployeeAsync(employeeId, cancellationToken);
        }

        /// <summary>
        /// Retrieves the full time entry history for an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of time entries.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        public async Task<IReadOnlyList<TimeEntry>> GetHistoryByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            if (employeeId == Guid.Empty)
            {
                throw new ValidationException("EmployeeId is required.");
            }

            return await _timeEntryRepository.GetHistoryByEmployeeAsync(employeeId, cancellationToken);
        }

        /// <summary>
        /// Soft-deletes a time entry by setting its active flag to false. The record is preserved for payroll audit purposes.
        /// </summary>
        /// <param name="timeEntryId">The time entry identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ValidationException">Thrown when the identifier is empty.</exception>
        /// <exception cref="NotFoundException">Thrown when the time entry does not exist.</exception>
        public async Task DeleteAsync(Guid timeEntryId, CancellationToken cancellationToken = default)
        {
            if (timeEntryId == Guid.Empty)
            {
                throw new ValidationException("Time entry id is required.");
            }

            var timeEntry = await _timeEntryRepository.GetByIdAsync(timeEntryId, cancellationToken);
            if (timeEntry == null)
            {
                throw new NotFoundException($"Time entry with id '{timeEntryId}' was not found.");
            }

            timeEntry.Deactivate();
            await _timeEntryRepository.UpdateAsync(timeEntry, cancellationToken);
        }
    }
}
