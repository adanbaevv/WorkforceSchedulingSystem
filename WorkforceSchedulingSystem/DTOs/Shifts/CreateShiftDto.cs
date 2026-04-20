namespace API.Dtos.Shifts
{
    public class CreateShiftDto
    {
        public DateOnly Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid? EmployeeId { get; set; }
    }
}
