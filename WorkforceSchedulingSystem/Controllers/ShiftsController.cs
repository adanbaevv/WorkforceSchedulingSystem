using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/shifts")]
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
                return NotFound();

            return Ok(shift);
        }

        // POST: api/shifts
        [HttpPost]
        public IActionResult Create([FromBody] CreateShiftRequest request)
        {
            var shift = new Shift(
                request.Date,
                request.StartTime,
                request.EndTime
            );

            _shiftRepository.Add(shift);
            return Ok(shift);
        }

        // PUT: api/shifts/{id}/assign/{employeeId}
        [HttpPut("{id}/assign/{employeeId}")]
        public IActionResult AssignEmployee(Guid id, Guid employeeId)
        {
            var shift = _shiftRepository.GetById(id);
            if (shift == null)
                return NotFound();

            shift.AssignEmployee(employeeId);
            _shiftRepository.Update(shift);

            return Ok();
        }

        // PUT: api/shifts/{id}/open
        [HttpPut("{id}/open")]
        public IActionResult OpenForPickup(Guid id)
        {
            var shift = _shiftRepository.GetById(id);
            if (shift == null)
                return NotFound();

            shift.OpenForPickup();
            _shiftRepository.Update(shift);

            return Ok();
        }
    }

    // DTO (keep it simple and local)
    public class CreateShiftRequest
    {
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
