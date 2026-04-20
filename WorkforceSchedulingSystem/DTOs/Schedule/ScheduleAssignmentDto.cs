namespace API.Dtos.Schedule
{
    public class ScheduleAssignmentDto
    {
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public double DurationHours { get; set; }
    }
}
