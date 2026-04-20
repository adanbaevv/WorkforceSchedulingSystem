using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Domain.Entities;

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
            if (shift == null)
            {
                throw new NotFoundException($"Shift with id '{shiftId}' was not found.");
            }

            shift.AssignEmployee(employeeId);
            await _shiftRepository.UpdateAsync(shift, cancellationToken);
        }

        public async Task OpenShiftAsync(Shift shift, CancellationToken cancellationToken = default)
        {
            if (shift == null)
            {
                throw new ValidationException("Shift is required.");
            }

            await _shiftRepository.AddAsync(shift, cancellationToken);
        }
    }
}
