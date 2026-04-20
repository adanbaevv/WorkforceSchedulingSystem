using Domain.Enums;

namespace API.Dtos.ShiftRequests
{
    public class ShiftRequestDto
    {
        public Guid Id { get; set; }
        public Guid ShiftId { get; set; }
        public Guid RequestedByEmployeeId { get; set; }
        public ShiftRequestType RequestType { get; set; }
        public ShiftRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? Reason { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? TenantId { get; set; }
    }
}
