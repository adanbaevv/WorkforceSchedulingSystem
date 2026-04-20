using Application.Common.Exceptions;
using Application.Interfaces.Repositories;

namespace Application.Services
{
    public class ShiftRequestService
    {
        private readonly IShiftRequestRepository _requestRepository;

        public ShiftRequestService(IShiftRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task ApproveRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
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
