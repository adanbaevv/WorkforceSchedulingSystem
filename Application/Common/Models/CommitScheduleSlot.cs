namespace Application.Common.Models
{
    public class CommitScheduleSlot
    {
        public DateOnly Date { get; }
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public Guid? AssignedEmployeeId { get; }

        public CommitScheduleSlot(
            DateOnly date,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid? assignedEmployeeId)
        {
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            AssignedEmployeeId = assignedEmployeeId;
        }
    }
}
