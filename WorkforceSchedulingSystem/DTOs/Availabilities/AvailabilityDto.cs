namespace API.Dtos.Availabilities
{
    public class AvailabilityDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? TenantId { get; set; }
    }
}
