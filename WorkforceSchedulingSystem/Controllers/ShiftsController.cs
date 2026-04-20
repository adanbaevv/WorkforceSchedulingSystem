using Asp.Versioning;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftRepository _shiftRepository;

        public ShiftsController(IShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        // GET: api/shifts/open
        [HttpGet("open")]
        public IActionResult GetOpenShifts()
        {
            var shifts = _shiftRepository.GetOpenShifts();
            return Ok(shifts);
        }

        // GET: api/shifts/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var shift = _shiftRepository.GetById(id);
            if (shift == null)
            {
                return NotFound();
            }

            return Ok(shift);
        }

        // POST: api/shifts
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateShiftRequest request,
            CancellationToken cancellationToken)
        {
            var shift = new Shift(
                request.Date,
                request.StartTime,
                request.EndTime);

            await _shiftRepository.AddAsync(shift, cancellationToken);
            return Ok(shift);
        }

        // PUT: api/shifts/{id}/assign/{employeeId}
        [HttpPut("{id}/assign/{employeeId}")]
        public async Task<IActionResult> AssignEmployee(
            Guid id,
            Guid employeeId,
            CancellationToken cancellationToken)
        {
            var shift = _shiftRepository.GetById(id);
            if (shift == null)
            {
                return NotFound();
            }

            shift.AssignEmployee(employeeId);
            await _shiftRepository.UpdateAsync(shift, cancellationToken);

            return Ok();
        }

        // PUT: api/shifts/{id}/open
        [HttpPut("{id}/open")]
        public async Task<IActionResult> OpenForPickup(Guid id, CancellationToken cancellationToken)
        {
            var shift = _shiftRepository.GetById(id);
            if (shift == null)
            {
                return NotFound();
            }

            shift.OpenForPickup();
            await _shiftRepository.UpdateAsync(shift, cancellationToken);

            return Ok();
        }
    }

    public class CreateShiftRequest
    {
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
