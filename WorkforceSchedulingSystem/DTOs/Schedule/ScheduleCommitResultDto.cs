namespace API.Dtos.Schedule
{
    public class ScheduleCommitResultDto
    {
        public int AssignedShiftsCreated { get; set; }
        public int OpenShiftsCreated { get; set; }
        public List<Guid> CreatedShiftIds { get; set; } = new();
    }
}
