using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Availability
    {
        public Guid Id { get; private set; }
        public Guid EmployeeId { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        protected Availability() { }

        public Availability(Guid employeeId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
                throw new ArgumentException("Start time must be before end time");

            Id = Guid.NewGuid();
            EmployeeId = employeeId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
        }
    }

}
