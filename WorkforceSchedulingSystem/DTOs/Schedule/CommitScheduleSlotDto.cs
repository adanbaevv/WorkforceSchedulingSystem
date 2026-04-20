namespace API.Dtos.Schedule
{
    public class CommitScheduleSlotDto
    {
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid? AssignedEmployeeId { get; set; }
    }
}
