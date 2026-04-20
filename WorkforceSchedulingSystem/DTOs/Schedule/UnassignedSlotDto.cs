namespace API.Dtos.Schedule
{
    public class UnassignedSlotDto
    {
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
