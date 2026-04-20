using Domain.Enums;

namespace API.Dtos.Shifts
{
    public class ShiftDto
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public ShiftStatus Status { get; set; }
        public Guid? AssignedEmployeeId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? TenantId { get; set; }
    }
}
