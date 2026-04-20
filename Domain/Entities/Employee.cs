using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public EmployeeRole Role { get; private set; }

        protected Employee() { } // for EF

        public Employee(string fullName, string email, EmployeeRole role)
        {
            FullName = fullName;
            Email = email;
            Role = role;
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
