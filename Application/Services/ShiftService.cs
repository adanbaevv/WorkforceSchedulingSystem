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

        public async Task AssignEmployeeAsync(
            Guid shiftId,
            Guid employeeId,
            CancellationToken cancellationToken = default)
        {
            var shift = _shiftRepository.GetById(shiftId);
            shift.AssignEmployee(employeeId);
            await _shiftRepository.UpdateAsync(shift, cancellationToken);
        }

        public async Task OpenShiftAsync(Shift shift, CancellationToken cancellationToken = default)
        {
            await _shiftRepository.AddAsync(shift, cancellationToken);
        }
    }
}
