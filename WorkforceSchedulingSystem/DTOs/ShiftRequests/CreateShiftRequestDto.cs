using Domain.Enums;

namespace API.Dtos.ShiftRequests
{
    public class CreateShiftRequestDto
    {
        public Guid EmployeeId { get; set; }
        public Guid ShiftId { get; set; }
        public ShiftRequestType Type { get; set; }
        public string? Reason { get; set; }
    }
}
