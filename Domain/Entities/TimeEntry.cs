using Domain.Common;

namespace Domain.Entities
{
    public class TimeEntry : BaseEntity
    {
        public Guid EmployeeId { get; private set; }
        public Guid? ShiftId { get; private set; }
        public DateTime ClockInAt { get; private set; }
        public DateTime? ClockOutAt { get; private set; }
        public string? Notes { get; private set; }

        public Employee Employee { get; private set; } = null!;
        public Shift? Shift { get; private set; }

        protected TimeEntry() { }

        public TimeEntry(Guid employeeId, Guid? shiftId, DateTime clockInAt)
        {
            EmployeeId = employeeId;
            ShiftId = shiftId;
            ClockInAt = clockInAt;
        }

        public void ClockOut(DateTime clockOutAt, string? notes)
        {
            ClockOutAt = clockOutAt;
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }
    }
}
