using Asp.Versioning;
using Application.Interfaces.Services;
using API.Dtos.TimeEntries;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TimeEntriesController : ControllerBase
    {
        private readonly ITimeEntryService _timeEntryService;

        public TimeEntriesController(ITimeEntryService timeEntryService)
        {
            _timeEntryService = timeEntryService;
        }

        /// <summary>
        /// Clocks an employee in and creates a new time entry.
        /// </summary>
        /// <param name="dto">The clock-in payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created time entry.</returns>
        [HttpPost("clock-in")]
        public async Task<ActionResult<TimeEntryDto>> ClockIn(
            [FromBody] ClockInDto dto,
            CancellationToken cancellationToken)
        {
            var timeEntry = await _timeEntryService.ClockInAsync(dto.EmployeeId, dto.ShiftId, cancellationToken);
            var timeEntryDto = MapToDto(timeEntry);

            return CreatedAtAction(nameof(GetActiveByEmployee), new { employeeId = timeEntryDto.EmployeeId, version = "1.0" }, timeEntryDto);
        }

        /// <summary>
        /// Clocks a time entry out and stores optional notes.
        /// </summary>
        /// <param name="id">The time entry identifier.</param>
        /// <param name="dto">The clock-out payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated time entry.</returns>
        [HttpPost("{id:guid}/clock-out")]
        public async Task<ActionResult<TimeEntryDto>> ClockOut(
            Guid id,
            [FromBody] ClockOutDto dto,
            CancellationToken cancellationToken)
        {
            var timeEntry = await _timeEntryService.ClockOutAsync(id, dto.Notes, cancellationToken);
            return Ok(MapToDto(timeEntry));
        }

        /// <summary>
        /// Retrieves the active time entry for an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The active time entry, or no content when none exists.</returns>
        [HttpGet("employee/{employeeId:guid}/active")]
        public async Task<ActionResult<TimeEntryDto>> GetActiveByEmployee(
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            var timeEntry = await _timeEntryService.GetActiveByEmployeeAsync(employeeId, cancellationToken);
            if (timeEntry == null)
            {
                return NoContent();
            }

            return Ok(MapToDto(timeEntry));
        }

        /// <summary>
        /// Retrieves the time entry history for an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of time entries.</returns>
        [HttpGet("employee/{employeeId:guid}")]
        public async Task<ActionResult<IReadOnlyList<TimeEntryDto>>> GetHistoryByEmployee(
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            var entries = await _timeEntryService.GetHistoryByEmployeeAsync(employeeId, cancellationToken);
            return Ok(entries.Select(MapToDto).ToList());
        }

        private static TimeEntryDto MapToDto(TimeEntry timeEntry)
        {
            return new TimeEntryDto
            {
                Id = timeEntry.Id,
                EmployeeId = timeEntry.EmployeeId,
                ShiftId = timeEntry.ShiftId,
                ClockInAt = timeEntry.ClockInAt,
                ClockOutAt = timeEntry.ClockOutAt,
                Notes = timeEntry.Notes,
                IsActive = timeEntry.IsActive,
                CreatedAt = timeEntry.CreatedAt,
                UpdatedAt = timeEntry.UpdatedAt,
                TenantId = timeEntry.TenantId
            };
        }
    }
}
