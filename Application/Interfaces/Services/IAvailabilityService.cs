using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IAvailabilityService
    {
        /// <summary>
        /// Retrieves all availability records.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of availability records.</returns>
        Task<IReadOnlyList<Availability>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an availability record by identifier.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested availability record.</returns>
        Task<Availability> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves every availability record for a given employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of availability records for the employee.</returns>
        Task<IReadOnlyList<Availability>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new availability record for an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="dayOfWeek">The day of the week the availability applies to.</param>
        /// <param name="startTime">The start-of-day time the employee is available.</param>
        /// <param name="endTime">The end-of-day time the employee is available.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created availability record.</returns>
        Task<Availability> CreateAsync(
            Guid employeeId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing availability record.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="dayOfWeek">The updated day of the week.</param>
        /// <param name="startTime">The updated start-of-day time.</param>
        /// <param name="endTime">The updated end-of-day time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated availability record.</returns>
        Task<Availability> UpdateAsync(
            Guid id,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an availability record.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
