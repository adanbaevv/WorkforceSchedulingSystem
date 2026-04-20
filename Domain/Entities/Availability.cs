using Domain.Common;

namespace Domain.Entities
{
    public class Availability : BaseEntity
    {
        public Guid EmployeeId { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        protected Availability() { }

        public Availability(Guid employeeId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
            {
                throw new ArgumentException("Start time must be before end time");
            }

            EmployeeId = employeeId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
