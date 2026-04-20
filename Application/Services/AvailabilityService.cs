using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public AvailabilityService(
            IAvailabilityRepository availabilityRepository,
            IEmployeeRepository employeeRepository)
        {
            _availabilityRepository = availabilityRepository;
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Retrieves all availability records.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of availability records.</returns>
        public async Task<IReadOnlyList<Availability>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _availabilityRepository.GetAllAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves an availability record by identifier.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested availability record.</returns>
        /// <exception cref="NotFoundException">Thrown when the availability record does not exist.</exception>
        public async Task<Availability> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var availability = await _availabilityRepository.GetByIdAsync(id, cancellationToken);
            return availability ?? throw new NotFoundException($"Availability with id '{id}' was not found.");
        }

        /// <summary>
        /// Retrieves every availability record for a given employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of availability records for the employee.</returns>
        /// <exception cref="ValidationException">Thrown when the employee identifier is empty.</exception>
        /// <exception cref="NotFoundException">Thrown when the employee does not exist.</exception>
        public async Task<IReadOnlyList<Availability>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
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

            return await _availabilityRepository.GetByEmployeeAsync(employeeId, cancellationToken);
        }

        /// <summary>
        /// Creates a new availability record for an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="dayOfWeek">The day of the week the availability applies to.</param>
        /// <param name="startTime">The start-of-day time the employee is available.</param>
        /// <param name="endTime">The end-of-day time the employee is available.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created availability record.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the employee does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when the employee already has an overlapping availability window on the same day.</exception>
        public async Task<Availability> CreateAsync(
            Guid employeeId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            CancellationToken cancellationToken = default)
        {
            ValidateInput(employeeId, startTime, endTime);

            var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
            if (employee == null)
            {
                throw new NotFoundException($"Employee with id '{employeeId}' was not found.");
            }

            var existing = await _availabilityRepository.GetByEmployeeAsync(employeeId, cancellationToken);
            if (HasOverlap(existing, dayOfWeek, startTime, endTime, excludeId: null))
            {
                throw new ConflictException("The employee already has an overlapping availability window on this day.");
            }

            var availability = new Availability(employeeId, dayOfWeek, startTime, endTime);
            await _availabilityRepository.AddAsync(availability, cancellationToken);

            return availability;
        }

        /// <summary>
        /// Updates an existing availability record.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="dayOfWeek">The updated day of the week.</param>
        /// <param name="startTime">The updated start-of-day time.</param>
        /// <param name="endTime">The updated end-of-day time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated availability record.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the availability record does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when the updated window overlaps another availability for the employee.</exception>
        public async Task<Availability> UpdateAsync(
            Guid id,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            CancellationToken cancellationToken = default)
        {
            if (startTime >= endTime)
            {
                throw new ValidationException("Start time must be before end time.");
            }

            var availability = await GetByIdAsync(id, cancellationToken);

            var existing = await _availabilityRepository.GetByEmployeeAsync(availability.EmployeeId, cancellationToken);
            if (HasOverlap(existing, dayOfWeek, startTime, endTime, excludeId: id))
            {
                throw new ConflictException("The employee already has an overlapping availability window on this day.");
            }

            availability.Update(dayOfWeek, startTime, endTime);
            await _availabilityRepository.UpdateAsync(availability, cancellationToken);

            return availability;
        }

        /// <summary>
        /// Soft-deletes an availability record by setting its active flag to false.
        /// Historical records are preserved; subsequent reads will treat the record as absent.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="NotFoundException">Thrown when the availability record does not exist.</exception>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var availability = await GetByIdAsync(id, cancellationToken);
            availability.Deactivate();
            await _availabilityRepository.UpdateAsync(availability, cancellationToken);
        }

        private static void ValidateInput(Guid employeeId, TimeSpan startTime, TimeSpan endTime)
        {
            if (employeeId == Guid.Empty)
            {
                throw new ValidationException("EmployeeId is required.");
            }

            if (startTime >= endTime)
            {
                throw new ValidationException("Start time must be before end time.");
            }
        }

        private static bool HasOverlap(
            IReadOnlyList<Availability> existing,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid? excludeId)
        {
            foreach (var availability in existing)
            {
                if (excludeId.HasValue && availability.Id == excludeId.Value)
                {
                    continue;
                }

                if (availability.DayOfWeek != dayOfWeek)
                {
                    continue;
                }

                if (availability.StartTime < endTime && startTime < availability.EndTime)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
