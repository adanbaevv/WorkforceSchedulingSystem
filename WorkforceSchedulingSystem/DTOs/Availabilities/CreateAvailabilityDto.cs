namespace API.Dtos.Availabilities
{
    public class CreateAvailabilityDto
    {
        public Guid EmployeeId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
