using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IShiftRequestRepository
    {
        ShiftRequest GetById(Guid id);
        IEnumerable<ShiftRequest> GetPending();
        void Add(ShiftRequest request);
        void Update(ShiftRequest request);
    }
}
