namespace Application.Common.Models
{
    public class ShiftSlot
    {
        public DateOnly Date { get; }
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }

        public ShiftSlot(DateOnly date, TimeSpan startTime, TimeSpan endTime)
        {
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
        }

        public double DurationHours => (EndTime - StartTime).TotalHours;
    }
}
