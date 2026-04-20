using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class ShiftRequest : BaseEntity
    {
        public Guid ShiftId { get; private set; }
        public Guid RequestedByEmployeeId { get; private set; }
        public ShiftRequestType RequestType { get; private set; }
        public ShiftRequestStatus Status { get; private set; }
        public DateTime RequestedAt { get; private set; }
        public string? Reason { get; private set; }

        protected ShiftRequest() { }

        public ShiftRequest(Guid shiftId, Guid employeeId, ShiftRequestType type, string? reason = null)
        {
            ShiftId = shiftId;
            RequestedByEmployeeId = employeeId;
            RequestType = type;
            Status = ShiftRequestStatus.Pending;
            RequestedAt = DateTime.UtcNow;
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        }

        public void Approve()
        {
            Status = ShiftRequestStatus.Approved;
        }

        public void Reject()
        {
            Status = ShiftRequestStatus.Rejected;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
