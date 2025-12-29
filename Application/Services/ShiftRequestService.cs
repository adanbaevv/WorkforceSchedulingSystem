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

        public void ApproveRequest(Guid requestId)
        {
            var request = _requestRepository.GetById(requestId);
            request.Approve();
            _requestRepository.Update(request);
        }

        public void RejectRequest(Guid requestId)
        {
            var request = _requestRepository.GetById(requestId);
            request.Reject();
            _requestRepository.Update(request);
        }
    }
}
