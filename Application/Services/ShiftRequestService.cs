using Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            request.Approve();
            await _requestRepository.UpdateAsync(request, cancellationToken);
        }

        public async Task RejectRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            var request = _requestRepository.GetById(requestId);
            request.Reject();
            await _requestRepository.UpdateAsync(request, cancellationToken);
        }
    }
}
