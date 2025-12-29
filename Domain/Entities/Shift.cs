using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Shift
    {
        public Guid Id { get; private set; }
        public DateOnly Date { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public ShiftStatus Status { get; private set; }
        public Guid? AssignedEmployeeId { get; private set; }

        protected Shift() { }

        public Shift(DateOnly date, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
                throw new ArgumentException("Shift start time must be before end time");

            Id = Guid.NewGuid();
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
