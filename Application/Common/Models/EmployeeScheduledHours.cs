namespace Application.Common.Models
{
    public class EmployeeScheduledHours
    {
        public Guid EmployeeId { get; }
        public string EmployeeName { get; }
        public double ExistingHours { get; }
        public double NewlyAssignedHours { get; }
        public double TotalHours => ExistingHours + NewlyAssignedHours;

        public EmployeeScheduledHours(
            Guid employeeId,
            string employeeName,
            double existingHours,
            double newlyAssignedHours)
        {
            EmployeeId = employeeId;
            EmployeeName = employeeName;
            ExistingHours = existingHours;
            NewlyAssignedHours = newlyAssignedHours;
        }
    }
}
