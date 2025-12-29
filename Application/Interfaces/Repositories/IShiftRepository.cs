using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IShiftRepository
    {
        Shift GetById(Guid id);
        IEnumerable<Shift> GetOpenShifts();
        void Add(Shift shift);
        void Update(Shift shift);
    }
}
