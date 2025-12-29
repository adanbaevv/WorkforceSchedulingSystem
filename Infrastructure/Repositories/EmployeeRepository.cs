using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public Employee GetById(Guid id)
            => _context.Employees.Find(id);

        public IEnumerable<Employee> GetAll()
            => _context.Employees.ToList();

        public void Add(Employee employee)
            => _context.Employees.Add(employee);

        public void Update(Employee employee)
            => _context.Employees.Update(employee);
    }
}
