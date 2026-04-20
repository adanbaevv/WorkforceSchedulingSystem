using Asp.Versioning;
using Application.Interfaces.Services;
using API.Dtos.ShiftRequests;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ShiftRequestsController : ControllerBase
    {
        private readonly IShiftRequestService _shiftRequestService;

        public ShiftRequestsController(IShiftRequestService shiftRequestService)
        {
            _shiftRequestService = shiftRequestService;
        }

        /// <summary>
        /// Creates a new shift request with an optional reason.
        /// </summary>
        /// <param name="dto">The shift request payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created shift request.</returns>
        [HttpPost]
        public async Task<ActionResult<ShiftRequestDto>> Create(
            [FromBody] CreateShiftRequestDto dto,
            CancellationToken cancellationToken)
        {
            var request = await _shiftRequestService.CreateAsync(
                dto.EmployeeId,
                dto.ShiftId,
                dto.Type,
                dto.Reason,
                cancellationToken);

            var requestDto = MapToDto(request);
            return CreatedAtAction(nameof(GetByEmployee), new { employeeId = requestDto.RequestedByEmployeeId, version = "1.0" }, requestDto);
        }

        /// <summary>
        /// Retrieves all shift requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shift requests.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ShiftRequestDto>>> GetAll(CancellationToken cancellationToken)
        {
            var requests = await _shiftRequestService.GetAllAsync(cancellationToken);
            return Ok(requests.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Retrieves all pending shift requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of pending shift requests.</returns>
        [HttpGet("pending")]
        public async Task<ActionResult<IReadOnlyList<ShiftRequestDto>>> GetPending(CancellationToken cancellationToken)
        {
            var requests = await _shiftRequestService.GetPendingAsync(cancellationToken);
            return Ok(requests.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Retrieves all shift requests for a specific employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shift requests.</returns>
        [HttpGet("employee/{employeeId:guid}")]
        public async Task<ActionResult<IReadOnlyList<ShiftRequestDto>>> GetByEmployee(
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            var requests = await _shiftRequestService.GetByEmployeeAsync(employeeId, cancellationToken);
            return Ok(requests.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Approves a shift request.
        /// </summary>
        /// <param name="id">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift request.</returns>
        [HttpPost("{id:guid}/approve")]
        public async Task<ActionResult<ShiftRequestDto>> Approve(Guid id, CancellationToken cancellationToken)
        {
            var request = await _shiftRequestService.ApproveRequestAsync(id, cancellationToken);
            return Ok(MapToDto(request));
        }

        /// <summary>
        /// Rejects a shift request.
        /// </summary>
        /// <param name="id">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift request.</returns>
        [HttpPost("{id:guid}/reject")]
        public async Task<ActionResult<ShiftRequestDto>> Reject(Guid id, CancellationToken cancellationToken)
        {
            var request = await _shiftRequestService.RejectRequestAsync(id, cancellationToken);
            return Ok(MapToDto(request));
        }

        /// <summary>
        /// Soft delete (sets IsActive = false); historical records are preserved.
        /// This is distinct from rejecting a request: rejection changes the request's status to Rejected while
        /// keeping it visible in manager views, while deletion removes the record entirely from manager views
        /// (intended for cleaning up test or duplicate entries). After deletion, the request is excluded from
        /// every list and is no longer retrievable by id.
        /// </summary>
        /// <param name="id">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content when the deletion succeeds.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _shiftRequestService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        private static ShiftRequestDto MapToDto(ShiftRequest request)
        {
            return new ShiftRequestDto
            {
                Id = request.Id,
                ShiftId = request.ShiftId,
                RequestedByEmployeeId = request.RequestedByEmployeeId,
                RequestType = request.RequestType,
                Status = request.Status,
                RequestedAt = request.RequestedAt,
                Reason = request.Reason,
                IsActive = request.IsActive,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                TenantId = request.TenantId
            };
        }
    }
}
