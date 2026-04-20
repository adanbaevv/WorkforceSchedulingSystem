using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Shift : BaseEntity
    {
        public DateOnly Date { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public ShiftStatus Status { get; private set; }
        public Guid? AssignedEmployeeId { get; private set; }

        protected Shift() { }

        public Shift(DateOnly date, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
            {
                throw new ArgumentException("Shift start time must be before end time");
            }

            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            Status = ShiftStatus.Unassigned;
        }

        public void AssignEmployee(Guid employeeId)
        {
            AssignedEmployeeId = employeeId;
            Status = ShiftStatus.Assigned;
        }

        public void OpenForPickup()
        {
            Status = ShiftStatus.OpenForPickup;
            AssignedEmployeeId = null;
        }
    }
}
