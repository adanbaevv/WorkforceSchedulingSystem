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

        public ShiftRequestService(
            IShiftRequestRepository requestRepository,
            ICurrentUserService currentUserService)
        {
            _requestRepository = requestRepository;
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
        /// Approves a shift request.
        /// </summary>
        /// <param name="requestId">The shift request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated shift request.</returns>
        /// <exception cref="NotFoundException">Thrown when the shift request does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when the shift request is no longer pending.</exception>
        public async Task<ShiftRequest> ApproveRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            _ = _currentUserService.CurrentUserId;
            _ = _currentUserService.CurrentTenantId;

            var request = await GetExistingRequestAsync(requestId, cancellationToken);
            EnsurePending(request);

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
