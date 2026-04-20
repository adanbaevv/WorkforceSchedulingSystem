using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IShiftRepository _shiftRepository;

        public ShiftService(IShiftRepository shiftRepository, IEmployeeRepository employeeRepository)
        {
            _shiftRepository = shiftRepository;
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Retrieves all shifts ordered by date and start time.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shifts.</returns>
        public async Task<IReadOnlyList<Shift>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _shiftRepository.GetAllAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves all shifts currently open for pickup.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of open shifts.</returns>
        public async Task<IReadOnlyList<Shift>> GetOpenShiftsAsync(CancellationToken cancellationToken = default)
        {
            return await _shiftRepository.GetOpenShiftsAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves shifts in an inclusive date range.
        /// </summary>
        /// <param name="startDate">The range start date.</param>
        /// <param name="endDate">The range end date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shifts within the date range.</returns>
        /// <exception cref="ValidationException">Thrown when the date range is invalid.</exception>
        public async Task<IReadOnlyList<Shift>> GetByDateRangeAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            if (endDate < startDate)
            {
                throw new ValidationException("End date must be greater than or equal to start date.");
            }

            return await _shiftRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        }

        /// <summary>
        /// Retrieves shifts assigned to an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shifts for the employee.</returns>
        /// <exception cref="ValidationException">Thrown when the employee identifier is invalid.</exception>
        public async Task<IReadOnlyList<Shift>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            if (employeeId == Guid.Empty)
            {
                throw new ValidationException("EmployeeId is required.");
            }

            return await _shiftRepository.GetByEmployeeAsync(employeeId, cancellationToken);
        }

        /// <summary>
        /// Retrieves a shift by identifier.
        /// </summary>
        /// <param name="id">The shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested shift.</returns>
        /// <exception cref="NotFoundException">Thrown when the shift does not exist.</exception>
        public async Task<Shift> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var shift = await _shiftRepository.GetByIdAsync(id, cancellationToken);
            return shift ?? throw new NotFoundException($"Shift with id '{id}' was not found.");
        }

        /// <summary>
        /// Creates a new shift and either assigns it immediately or opens it for pickup.
        /// </summary>
        /// <param name="date">The shift date.</param>
        /// <param name="startTime">The shift start time.</param>
        /// <param name="endTime">The shift end time.</param>
        /// <param name="employeeId">The optional employee identifier to assign immediately.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created shift.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the provided employee does not exist.</exception>
        public async Task<Shift> CreateAsync(
            DateOnly date,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid? employeeId,
            CancellationToken cancellationToken = default)
        {
            if (startTime >= endTime)
            {
                throw new ValidationException("Shift start time must be before end time.");
            }

            var shift = new Shift(date, startTime, endTime);

            if (employeeId.HasValue)
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId.Value, cancellationToken);
                if (employee == null)
                {
                    throw new NotFoundException($"Employee with id '{employeeId.Value}' was not found.");
                }

                shift.AssignEmployee(employeeId.Value);
            }
            else
            {
                shift.OpenForPickup();
            }

            await _shiftRepository.AddAsync(shift, cancellationToken);

            return shift;
        }

        /// <summary>
        /// Assigns an employee to a shift.
        /// </summary>
        /// <param name="shiftId">The shift identifier.</param>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift.</returns>
        /// <exception cref="NotFoundException">Thrown when the shift does not exist.</exception>
        public async Task<Shift> AssignEmployeeAsync(
            Guid shiftId,
            Guid employeeId,
            CancellationToken cancellationToken = default)
        {
            var shift = await GetByIdAsync(shiftId, cancellationToken);
            shift.AssignEmployee(employeeId);
            await _shiftRepository.UpdateAsync(shift, cancellationToken);

            return shift;
        }

        /// <summary>
        /// Opens a shift for pickup.
        /// </summary>
        /// <param name="shiftId">The shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift.</returns>
        /// <exception cref="NotFoundException">Thrown when the shift does not exist.</exception>
        public async Task<Shift> OpenShiftAsync(Guid shiftId, CancellationToken cancellationToken = default)
        {
            var shift = await GetByIdAsync(shiftId, cancellationToken);
            shift.OpenForPickup();
            await _shiftRepository.UpdateAsync(shift, cancellationToken);

            return shift;
        }

        /// <summary>
        /// Soft-deletes a shift by setting its active flag to false. The record is preserved for audit purposes.
        /// </summary>
        /// <param name="shiftId">The shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="NotFoundException">Thrown when the shift does not exist.</exception>
        public async Task DeleteAsync(Guid shiftId, CancellationToken cancellationToken = default)
        {
            var shift = await GetByIdAsync(shiftId, cancellationToken);
            shift.Deactivate();
            await _shiftRepository.UpdateAsync(shift, cancellationToken);
        }
    }
}
