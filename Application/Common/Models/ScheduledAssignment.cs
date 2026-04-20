namespace Application.Common.Models
{
    public class ScheduledAssignment
    {
        public DateOnly Date { get; }
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public Guid EmployeeId { get; }
        public string EmployeeName { get; }
        public double DurationHours { get; }

        public ScheduledAssignment(
            DateOnly date,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid employeeId,
            string employeeName,
            double durationHours)
        {
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            EmployeeId = employeeId;
            EmployeeName = employeeName;
            DurationHours = durationHours;
        }
    }
}
