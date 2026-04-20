namespace API.Dtos.Schedule
{
    public class ShiftSlotDto
    {
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
