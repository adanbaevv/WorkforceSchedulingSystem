using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IShiftService
    {
        /// <summary>
        /// Retrieves all shifts ordered by date and start time.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shifts.</returns>
        Task<IReadOnlyList<Shift>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all shifts currently open for pickup.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of open shifts.</returns>
        Task<IReadOnlyList<Shift>> GetOpenShiftsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves shifts in an inclusive date range.
        /// </summary>
        /// <param name="startDate">The range start date.</param>
        /// <param name="endDate">The range end date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shifts within the date range.</returns>
        Task<IReadOnlyList<Shift>> GetByDateRangeAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves shifts assigned to an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shifts for the employee.</returns>
        Task<IReadOnlyList<Shift>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a shift by identifier.
        /// </summary>
        /// <param name="id">The shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested shift.</returns>
        Task<Shift> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new shift.
        /// </summary>
        /// <param name="date">The shift date.</param>
        /// <param name="startTime">The shift start time.</param>
        /// <param name="endTime">The shift end time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created shift.</returns>
        Task<Shift> CreateAsync(
            DateOnly date,
            TimeSpan startTime,
            TimeSpan endTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Assigns an employee to a shift.
        /// </summary>
        /// <param name="shiftId">The shift identifier.</param>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<Shift> AssignEmployeeAsync(
            Guid shiftId,
            Guid employeeId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens a shift for pickup.
        /// </summary>
        /// <param name="shiftId">The shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift.</returns>
        Task<Shift> OpenShiftAsync(Guid shiftId, CancellationToken cancellationToken = default);
    }
}
