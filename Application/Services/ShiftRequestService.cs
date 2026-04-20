using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Interfaces.Repositories;

namespace Application.Services
{
    public class ShiftRequestService
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

        public async Task ApproveRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            _ = _currentUserService.CurrentUserId;
            _ = _currentUserService.CurrentTenantId;

            var request = _requestRepository.GetById(requestId);
            if (request == null)
            {
                throw new NotFoundException($"Shift request with id '{requestId}' was not found.");
            }

            request.Approve();
            await _requestRepository.UpdateAsync(request, cancellationToken);
        }

        public async Task RejectRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            _ = _currentUserService.CurrentUserId;
            _ = _currentUserService.CurrentTenantId;

            var request = _requestRepository.GetById(requestId);
            if (request == null)
            {
                throw new NotFoundException($"Shift request with id '{requestId}' was not found.");
            }

            request.Reject();
            await _requestRepository.UpdateAsync(request, cancellationToken);
        }
    }
}
