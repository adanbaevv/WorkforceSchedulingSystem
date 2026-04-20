using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        /// <summary>
        /// Retrieves all employees.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of employees.</returns>
        Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an employee by identifier.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested employee.</returns>
        Task<Employee> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        /// <param name="fullName">The employee full name.</param>
        /// <param name="email">The employee email address.</param>
        /// <param name="role">The employee role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created employee.</returns>
        Task<Employee> CreateAsync(
            string fullName,
            string email,
            EmployeeRole role,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="fullName">The employee full name.</param>
        /// <param name="email">The employee email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        Task<Employee> UpdateAsync(
            Guid id,
            string fullName,
            string email,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deactivates an employee.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        Task<Employee> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Changes the role of an employee.
        /// </summary>
        /// <param name="id">The employee identifier.</param>
        /// <param name="role">The new role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated employee.</returns>
        Task<Employee> ChangeRoleAsync(
            Guid id,
            EmployeeRole role,
            CancellationToken cancellationToken = default);
    }
}
