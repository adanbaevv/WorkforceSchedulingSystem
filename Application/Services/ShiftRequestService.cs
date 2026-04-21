using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class ShiftRequestService : IShiftRequestService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IShiftRequestRepository _requestRepository;
        private readonly IShiftRepository _shiftRepository;

        public ShiftRequestService(
            IShiftRequestRepository requestRepository,
            IShiftRepository shiftRepository,
            ICurrentUserService currentUserService)
        {
            _requestRepository = requestRepository;
            _shiftRepository = shiftRepository;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Creates a new shift request with an optional reason.
        /// </summary>
        /// <param name="employeeId">The employee requesting the change.</param>
        /// <param name="shiftId">The shift identifier.</param>
        /// <param name="type">The request type.</param>
        /// <param name="reason">The optional request reason.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created shift request.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        public async Task<ShiftRequest> CreateAsync(
            Guid employeeId,
            Guid shiftId,
            ShiftRequestType type,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            _ = _currentUserService.CurrentUserId;
            _ = _currentUserService.CurrentTenantId;

            if (employeeId == Guid.Empty)
            {
                throw new ValidationException("EmployeeId is required.");
            }

            if (shiftId == Guid.Empty)
            {
                throw new ValidationException("ShiftId is required.");
            }

            if (reason?.Length > 500)
            {
                throw new ValidationException("Reason must be 500 characters or fewer.");
            }

            var request = new ShiftRequest(shiftId, employeeId, type, reason);
            await _requestRepository.AddAsync(request, cancellationToken);

            return request;
        }

        /// <summary>
        /// Retrieves all shift requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shift requests.</returns>
        public async Task<IReadOnlyList<ShiftRequest>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _requestRepository.GetAllAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves all pending shift requests.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of pending shift requests.</returns>
        public async Task<IReadOnlyList<ShiftRequest>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            return await _requestRepository.GetPendingAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves all shift requests for an employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of shift requests for the employee.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        public async Task<IReadOnlyList<ShiftRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            if (employeeId == Guid.Empty)
            {
                throw new ValidationException("EmployeeId is required.");
            }

            return await _requestRepository.GetByEmployeeAsync(employeeId, cancellationToken);
        }

        /// <summary>
        /// Approves a pending shift request and mutates the underlying shift to reflect the approval.
        /// Behavior depends on <see cref="ShiftRequest.RequestType"/>:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <b>Pickup</b> — the referenced shift must currently be open (unassigned). On approval the shift is
        /// assigned to the requesting employee via <see cref="Shift.AssignEmployee"/>. If the shift is already
        /// assigned to someone else, a <see cref="ConflictException"/> is thrown — the request cannot be
        /// granted because the slot is taken.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <b>Drop</b> — the referenced shift must currently be assigned to the requesting employee. On
        /// approval the shift is unassigned and opened for pickup via <see cref="Shift.OpenForPickup"/>.
        /// If the shift is already unassigned or assigned to a different employee, a
        /// <see cref="ConflictException"/> is thrown.
        /// </description>
        /// </item>
        /// </list>
        /// Both the shift mutation and the request status change are persisted. This is two separate
        /// <c>SaveChangesAsync</c> calls today — acceptable for the diploma demo. A future Unit-of-Work
        /// wrapper should make the pair transactional so a crash between them cannot leave the DB with the
        /// shift reassigned but the request still marked pending.
        /// </summary>
        /// <param name="requestId">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated (approved) shift request.</returns>
        /// <exception cref="ValidationException">Thrown when the identifier is empty.</exception>
        /// <exception cref="NotFoundException">Thrown when the request or its shift does not exist.</exception>
        /// <exception cref="ConflictException">
        /// Thrown when the request is not pending, or when the shift is in a state that makes the approval
        /// impossible (Pickup on an already-assigned shift, or Drop on a shift no longer owned by the requester).
        /// </exception>
        public async Task<ShiftRequest> ApproveRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            _ = _currentUserService.CurrentUserId;
            _ = _currentUserService.CurrentTenantId;

            var request = await GetExistingRequestAsync(requestId, cancellationToken);
            EnsurePending(request);

            var shift = await _shiftRepository.GetByIdAsync(request.ShiftId, cancellationToken);
            if (shift == null)
            {
                throw new NotFoundException("Shift associated with this request no longer exists.");
            }

            switch (request.RequestType)
            {
                case ShiftRequestType.Pickup:
                    if (shift.AssignedEmployeeId.HasValue)
                    {
                        throw new ConflictException("Shift is no longer open — it has already been assigned.");
                    }
                    shift.AssignEmployee(request.RequestedByEmployeeId);
                    break;

                case ShiftRequestType.Drop:
                    if (shift.AssignedEmployeeId != request.RequestedByEmployeeId)
                    {
                        throw new ConflictException("Shift is no longer assigned to this employee.");
                    }
                    shift.OpenForPickup();
                    break;

                default:
                    throw new ValidationException($"Unsupported request type '{request.RequestType}'.");
            }

            await _shiftRepository.UpdateAsync(shift, cancellationToken);

            request.Approve();
            await _requestRepository.UpdateAsync(request, cancellationToken);

            return request;
        }

        /// <summary>
        /// Rejects a shift request.
        /// </summary>
        /// <param name="requestId">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift request.</returns>
        /// <exception cref="NotFoundException">Thrown when the shift request does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when the shift request is no longer pending.</exception>
        public async Task<ShiftRequest> RejectRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            _ = _currentUserService.CurrentUserId;
            _ = _currentUserService.CurrentTenantId;

            var request = await GetExistingRequestAsync(requestId, cancellationToken);
            EnsurePending(request);

            request.Reject();
            await _requestRepository.UpdateAsync(request, cancellationToken);

            return request;
        }

        /// <summary>
        /// Soft-deletes a shift request by setting its active flag to false. The record is preserved for audit purposes.
        /// </summary>
        /// <param name="requestId">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ValidationException">Thrown when the identifier is empty.</exception>
        /// <exception cref="NotFoundException">Thrown when the shift request does not exist.</exception>
        public async Task DeleteAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            var request = await GetExistingRequestAsync(requestId, cancellationToken);
            request.Deactivate();
            await _requestRepository.UpdateAsync(request, cancellationToken);
        }

        private async Task<ShiftRequest> GetExistingRequestAsync(Guid requestId, CancellationToken cancellationToken)
        {
            if (requestId == Guid.Empty)
            {
                throw new ValidationException("Request id is required.");
            }

            var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
            return request ?? throw new NotFoundException($"Shift request with id '{requestId}' was not found.");
        }

        private static void EnsurePending(ShiftRequest request)
        {
            if (request.Status != ShiftRequestStatus.Pending)
            {
                throw new ConflictException("Only pending shift requests can be updated.");
            }
        }
    }
}
