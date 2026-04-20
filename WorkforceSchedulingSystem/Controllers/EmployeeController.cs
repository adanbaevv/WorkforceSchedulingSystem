using Application.Interfaces.Repositories;
using API.Dtos.Employees;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeesController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // GET: api/employees
        [HttpGet]
        public IActionResult GetAll()
        {
            var employees = _employeeRepository.GetAll();
            return Ok(employees);
        }

        // GET: api/employees/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var employee = _employeeRepository.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        // POST: api/employees
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmployeeDto dto,
            CancellationToken cancellationToken)
        {
            var employee = new Employee(dto.FullName, dto.Email, dto.Role);

            await _employeeRepository.AddAsync(employee, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        // PUT: api/employees/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateEmployeeDto dto,
            CancellationToken cancellationToken)
        {
            var employee = _employeeRepository.GetById(id);

            if (employee == null)
                return NotFound();

            employee.UpdateProfile(dto.FullName, dto.Email); // DOMAIN LOGIC
            await _employeeRepository.UpdateAsync(employee, cancellationToken); // PERSISTENCE

            return NoContent();
        }

        // PUT: api/employees/{id}/role
        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeRole(
            Guid id,
            [FromBody] ChangeEmployeeRoleDto dto,
            CancellationToken cancellationToken)
        {
            var employee = _employeeRepository.GetById(id);

            if (employee == null)
                return NotFound();

            employee.ChangeRole(dto.Role);
            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            return NoContent();
        }

        // PUT: api/employees/{id}/deactivate
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        {
            var employee = _employeeRepository.GetById(id);

            if (employee == null)
                return NotFound();

            employee.Deactivate();
            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            return NoContent();
        }
    }
}
