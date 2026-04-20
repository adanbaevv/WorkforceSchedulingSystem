using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Retrieves all employees.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of employees.</returns>
        public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _employeeRepository.GetAllAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves an employee by identifier.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested employee.</returns>
        /// <exception cref="NotFoundException">Thrown when the employee does not exist.</exception>
        public async Task<Employee> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
            return employee ?? throw new NotFoundException($"Employee with id '{id}' was not found.");
        }

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        /// <param name="fullName">The employee full name.</param>
        /// <param name="email">The employee email address.</param>
        /// <param name="role">The employee role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created employee.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        public async Task<Employee> CreateAsync(
            string fullName,
            string email,
            EmployeeRole role,
            CancellationToken cancellationToken = default)
        {
            ValidateInput(fullName, email);

            var employee = new Employee(fullName.Trim(), email.Trim(), role);
            await _employeeRepository.AddAsync(employee, cancellationToken);

            return employee;
        }

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="fullName">The employee full name.</param>
        /// <param name="email">The employee email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        /// <exception cref="NotFoundException">Thrown when the employee does not exist.</exception>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        public async Task<Employee> UpdateAsync(
            Guid id,
            string fullName,
            string email,
            CancellationToken cancellationToken = default)
        {
            ValidateInput(fullName, email);

            var employee = await GetByIdAsync(id, cancellationToken);
            employee.UpdateProfile(fullName.Trim(), email.Trim());
            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            return employee;
        }

        /// <summary>
        /// Deactivates an employee.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        /// <exception cref="NotFoundException">Thrown when the employee does not exist.</exception>
        public async Task<Employee> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employee = await GetByIdAsync(id, cancellationToken);
            employee.Deactivate();
            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            return employee;
        }

        /// <summary>
        /// Changes the role of an employee.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="role">The new role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        /// <exception cref="NotFoundException">Thrown when the employee does not exist.</exception>
        public async Task<Employee> ChangeRoleAsync(
            Guid id,
            EmployeeRole role,
            CancellationToken cancellationToken = default)
        {
            var employee = await GetByIdAsync(id, cancellationToken);
            employee.ChangeRole(role);
            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            return employee;
        }

        private static void ValidateInput(string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ValidationException("Employee full name is required.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException("Employee email is required.");
            }

            if (!email.Contains('@'))
            {
                throw new ValidationException("Employee email must be a valid email address.");
            }
        }
    }
}
