namespace Application.Common.Models
{
    public class UnassignedSlot
    {
        public DateOnly Date { get; }
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public string Reason { get; }

        public UnassignedSlot(DateOnly date, TimeSpan startTime, TimeSpan endTime, string reason)
        {
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            Reason = reason;
        }
    }
}
