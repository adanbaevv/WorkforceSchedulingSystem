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
        public IActionResult Create([FromBody] CreateEmployeeDto dto)
        {
            var employee = new Employee(dto.FullName, dto.Email, dto.Role);

            _employeeRepository.Add(employee);

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        // PUT: api/employees/{id}
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UpdateEmployeeDto dto)
        {
            var employee = _employeeRepository.GetById(id);

            if (employee == null)
                return NotFound();

            employee.UpdateProfile(dto.FullName, dto.Email); // DOMAIN LOGIC
            _employeeRepository.Update(employee);            // PERSISTENCE

            return NoContent();
        }

        // PUT: api/employees/{id}/role
        [HttpPut("{id}/role")]
        public IActionResult ChangeRole(Guid id, [FromBody] ChangeEmployeeRoleDto dto)
        {
            var employee = _employeeRepository.GetById(id);

            if (employee == null)
                return NotFound();

            employee.ChangeRole(dto.Role);
            _employeeRepository.Update(employee);

            return NoContent();
        }

        // PUT: api/employees/{id}/deactivate
        [HttpPut("{id}/deactivate")]
        public IActionResult Deactivate(Guid id)
        {
            var employee = _employeeRepository.GetById(id);

            if (employee == null)
                return NotFound();

            employee.Deactivate();
            _employeeRepository.Update(employee);

            return NoContent();
        }
    }
}
