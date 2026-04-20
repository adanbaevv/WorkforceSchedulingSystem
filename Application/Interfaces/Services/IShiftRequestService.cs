using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IShiftRequestService
    {
        /// <summary>
        /// Creates a new shift request with an optional reason.
        /// </summary>
        /// <param name="employeeId">The employee requesting the change.</param>
        /// <param name="shiftId">The shift identifier.</param>
        /// <param name="type">The request type.</param>
        /// <param name="reason">The optional request reason.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created shift request.</returns>
        Task<ShiftRequest> CreateAsync(
            Guid employeeId,
            Guid shiftId,
            ShiftRequestType type,
            string? reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all shift requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shift requests.</returns>
        Task<IReadOnlyList<ShiftRequest>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all pending shift requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of pending shift requests.</returns>
        Task<IReadOnlyList<ShiftRequest>> GetPendingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all shift requests for an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shift requests for the employee.</returns>
        Task<IReadOnlyList<ShiftRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves a shift request.
        /// </summary>
        /// <param name="requestId">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift request.</returns>
        Task<ShiftRequest> ApproveRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rejects a shift request.
        /// </summary>
        /// <param name="requestId">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift request.</returns>
        Task<ShiftRequest> RejectRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft-deletes a shift request by setting its active flag to false. The record is preserved for audit purposes.
        /// </summary>
        /// <param name="requestId">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task DeleteAsync(Guid requestId, CancellationToken cancellationToken = default);
    }
}
