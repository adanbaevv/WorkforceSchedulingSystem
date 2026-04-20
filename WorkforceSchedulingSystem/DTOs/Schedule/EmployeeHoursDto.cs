namespace API.Dtos.Schedule
{
    public class EmployeeHoursDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public double ExistingHours { get; set; }
        public double NewlyAssignedHours { get; set; }
        public double TotalHours { get; set; }
    }
}
