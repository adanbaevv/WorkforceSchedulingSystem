using Asp.Versioning;
using Application.Interfaces.Services;
using API.Dtos.Shifts;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        /// <summary>
        /// Retrieves all shifts.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shifts.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ShiftDto>>> GetAll(CancellationToken cancellationToken)
        {
            var shifts = await _shiftService.GetAllAsync(cancellationToken);
            return Ok(shifts.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Retrieves all shifts within an inclusive date range.
        /// </summary>
        /// <param name="startDate">The range start date.</param>
        /// <param name="endDate">The range end date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shifts within the date range.</returns>
        [HttpGet("by-date-range")]
        public async Task<ActionResult<IReadOnlyList<ShiftDto>>> GetByDateRange(
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate,
            CancellationToken cancellationToken)
        {
            var shifts = await _shiftService.GetByDateRangeAsync(startDate, endDate, cancellationToken);
            return Ok(shifts.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Retrieves all shifts assigned to an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of employee shifts.</returns>
        [HttpGet("employee/{employeeId:guid}")]
        public async Task<ActionResult<IReadOnlyList<ShiftDto>>> GetByEmployee(
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            var shifts = await _shiftService.GetByEmployeeAsync(employeeId, cancellationToken);
            return Ok(shifts.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Retrieves all shifts open for pickup.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of open shifts.</returns>
        [HttpGet("open")]
        public async Task<ActionResult<IReadOnlyList<ShiftDto>>> GetOpenShifts(CancellationToken cancellationToken)
        {
            var shifts = await _shiftService.GetOpenShiftsAsync(cancellationToken);
            return Ok(shifts.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Retrieves a shift by identifier.
        /// </summary>
        /// <param name="id">The shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested shift.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ShiftDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var shift = await _shiftService.GetByIdAsync(id, cancellationToken);
            return Ok(MapToDto(shift));
        }

        /// <summary>
        /// Creates a new shift. When <paramref name="dto"/> includes an employee identifier,
        /// the shift is created and immediately assigned to that employee. When the employee identifier
        /// is omitted or empty, the shift is created in the open state and becomes available for pickup.
        /// </summary>
        /// <param name="dto">The shift creation payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created shift with its applied state.</returns>
        [HttpPost]
        public async Task<ActionResult<ShiftDto>> Create(
            [FromBody] CreateShiftDto dto,
            CancellationToken cancellationToken)
        {
            var shift = await _shiftService.CreateAsync(
                dto.Date,
                dto.StartTime,
                dto.EndTime,
                dto.EmployeeId,
                cancellationToken);

            var shiftDto = MapToDto(shift);
            return CreatedAtAction(nameof(GetById), new { id = shiftDto.Id, version = "1.0" }, shiftDto);
        }

        /// <summary>
        /// Assigns an employee to a shift.
        /// </summary>
        /// <param name="id">The shift identifier.</param>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift.</returns>
        [HttpPut("{id:guid}/assign/{employeeId:guid}")]
        public async Task<ActionResult<ShiftDto>> AssignEmployee(
            Guid id,
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            var shift = await _shiftService.AssignEmployeeAsync(id, employeeId, cancellationToken);
            return Ok(MapToDto(shift));
        }

        /// <summary>
        /// Opens a shift for pickup.
        /// </summary>
        /// <param name="id">The shift identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift.</returns>
        [HttpPut("{id:guid}/open")]
        public async Task<ActionResult<ShiftDto>> OpenForPickup(Guid id, CancellationToken cancellationToken)
        {
            var shift = await _shiftService.OpenShiftAsync(id, cancellationToken);
            return Ok(MapToDto(shift));
        }

        private static ShiftDto MapToDto(Shift shift)
        {
            return new ShiftDto
            {
                Id = shift.Id,
                Date = shift.Date,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Status = shift.Status,
                AssignedEmployeeId = shift.AssignedEmployeeId,
                IsActive = shift.IsActive,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt,
                TenantId = shift.TenantId
            };
        }
    }
}
