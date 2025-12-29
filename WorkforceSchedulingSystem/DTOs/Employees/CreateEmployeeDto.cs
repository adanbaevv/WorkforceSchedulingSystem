using Domain.Enums;

namespace API.Dtos.Employees
{
    public class CreateEmployeeDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public EmployeeRole Role { get; set; }
    }
}
