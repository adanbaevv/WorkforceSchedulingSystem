namespace API.Dtos.TimeEntries
{
    public class ClockInDto
    {
        public Guid EmployeeId { get; set; }
        public Guid? ShiftId { get; set; }
    }
}
