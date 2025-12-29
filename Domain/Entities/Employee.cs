using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public EmployeeRole Role { get; private set; }
        public bool IsActive { get; private set; }

        protected Employee() { } // for EF

        public Employee(string fullName, string email, EmployeeRole role)
        {
            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            Role = role;
            IsActive = true;
        }

        public void UpdateProfile(string fullName, string email)
        {
            FullName = fullName;
            Email = email;
        }

        public void ChangeRole(EmployeeRole role)
        {
            Role = role;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
