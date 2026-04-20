namespace API.Dtos.TimeEntries
{
    public class TimeEntryDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? ShiftId { get; set; }
        public DateTime ClockInAt { get; set; }
        public DateTime? ClockOutAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? TenantId { get; set; }
    }
}
