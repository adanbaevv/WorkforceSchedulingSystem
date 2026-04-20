using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        Employee GetById(Guid id);
        IEnumerable<Employee> GetAll();
        Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
        Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    }
}
