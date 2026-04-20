using Asp.Versioning;
using Application.Interfaces.Services;
using API.Dtos.Employees;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Retrieves all employees.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of employee records.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(CancellationToken cancellationToken)
        {
            var employees = await _employeeService.GetAllAsync(cancellationToken);
            return Ok(employees.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Retrieves a single employee by identifier.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested employee.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var employee = await _employeeService.GetByIdAsync(id, cancellationToken);
            return Ok(MapToDto(employee));
        }

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        /// <param name="dto">The employee creation payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created employee.</returns>
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> Create(
            [FromBody] CreateEmployeeDto dto,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeService.CreateAsync(
                dto.FullName,
                dto.Email,
                dto.Role,
                cancellationToken);

            var employeeDto = MapToDto(employee);
            return CreatedAtAction(nameof(GetById), new { id = employeeDto.Id, version = "1.0" }, employeeDto);
        }

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="dto">The employee update payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<EmployeeDto>> Update(
            Guid id,
            [FromBody] UpdateEmployeeDto dto,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeService.UpdateAsync(
                id,
                dto.FullName,
                dto.Email,
                cancellationToken);

            return Ok(MapToDto(employee));
        }

        /// <summary>
        /// Changes an employee role.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="dto">The role change payload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        [HttpPut("{id:guid}/role")]
        public async Task<ActionResult<EmployeeDto>> ChangeRole(
            Guid id,
            [FromBody] ChangeEmployeeRoleDto dto,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeService.ChangeRoleAsync(id, dto.Role, cancellationToken);
            return Ok(MapToDto(employee));
        }

        /// <summary>
        /// Deactivates an employee.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        [HttpPut("{id:guid}/deactivate")]
        public async Task<ActionResult<EmployeeDto>> Deactivate(Guid id, CancellationToken cancellationToken)
        {
            var employee = await _employeeService.DeleteAsync(id, cancellationToken);
            return Ok(MapToDto(employee));
        }

        /// <summary>
        /// Soft delete (sets IsActive = false); historical records are preserved.
        /// After deletion, the employee is excluded from every list and is no longer retrievable by id.
        /// Related records (shifts, time entries, shift requests) that depend on this employee are also
        /// hidden from read APIs while the underlying rows remain intact for audit purposes.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content when the deletion succeeds.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _employeeService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        private static EmployeeDto MapToDto(Employee employee)
        {
            return new EmployeeDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                Role = employee.Role,
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt,
                TenantId = employee.TenantId
            };
        }
    }
}
