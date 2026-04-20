using Asp.Versioning;
using Application.Interfaces.Services;
using API.Dtos.Availabilities;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AvailabilitiesController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilitiesController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        /// <summary>
        /// Retrieves all availability records.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of availability records.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AvailabilityDto>>> GetAll(CancellationToken cancellationToken)
        {
            var availabilities = await _availabilityService.GetAllAsync(cancellationToken);
            return Ok(availabilities.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Retrieves a single availability record by identifier.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested availability record.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AvailabilityDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var availability = await _availabilityService.GetByIdAsync(id, cancellationToken);
            return Ok(MapToDto(availability));
        }

        /// <summary>
        /// Retrieves every availability record for the specified employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of availability records.</returns>
        [HttpGet("employee/{employeeId:guid}")]
        public async Task<ActionResult<IReadOnlyList<AvailabilityDto>>> GetByEmployee(
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            var availabilities = await _availabilityService.GetByEmployeeAsync(employeeId, cancellationToken);
            return Ok(availabilities.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Creates a new availability record.
        /// </summary>
        /// <param name="dto">The availability creation payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created availability record.</returns>
        [HttpPost]
        public async Task<ActionResult<AvailabilityDto>> Create(
            [FromBody] CreateAvailabilityDto dto,
            CancellationToken cancellationToken)
        {
            var availability = await _availabilityService.CreateAsync(
                dto.EmployeeId,
                dto.DayOfWeek,
                dto.StartTime,
                dto.EndTime,
                cancellationToken);

            var availabilityDto = MapToDto(availability);
            return CreatedAtAction(nameof(GetById), new { id = availabilityDto.Id, version = "1.0" }, availabilityDto);
        }

        /// <summary>
        /// Updates an existing availability record.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="dto">The availability update payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated availability record.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<AvailabilityDto>> Update(
            Guid id,
            [FromBody] UpdateAvailabilityDto dto,
            CancellationToken cancellationToken)
        {
            var availability = await _availabilityService.UpdateAsync(
                id,
                dto.DayOfWeek,
                dto.StartTime,
                dto.EndTime,
                cancellationToken);

            return Ok(MapToDto(availability));
        }

        /// <summary>
        /// Soft delete (sets IsActive = false); historical records are preserved.
        /// After deletion, the availability is excluded from every list and is no longer retrievable by id,
        /// and the auto-scheduler will stop considering the window.
        /// </summary>
        /// <param name="id">The availability identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content when the deletion succeeds.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _availabilityService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        private static AvailabilityDto MapToDto(Availability availability)
        {
            return new AvailabilityDto
            {
                Id = availability.Id,
                EmployeeId = availability.EmployeeId,
                DayOfWeek = availability.DayOfWeek,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                IsActive = availability.IsActive,
                CreatedAt = availability.CreatedAt,
                UpdatedAt = availability.UpdatedAt,
                TenantId = availability.TenantId
            };
        }
    }
}
