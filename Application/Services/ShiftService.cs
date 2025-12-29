using Application.Interfaces.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ShiftService
    {
        private readonly IShiftRepository _shiftRepository;

        public ShiftService(IShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public void AssignEmployee(Guid shiftId, Guid employeeId)
        {
            var shift = _shiftRepository.GetById(shiftId);
            shift.AssignEmployee(employeeId);
            _shiftRepository.Update(shift);
        }

        public void OpenShift(Shift shift)
        {
            _shiftRepository.Add(shift);
        }
    }
}
