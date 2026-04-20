namespace API.Dtos.Schedule
{
    public class ScheduleProposalDto
    {
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public List<ScheduleAssignmentDto> Assignments { get; set; } = new();
        public List<UnassignedSlotDto> UnassignedSlots { get; set; } = new();
        public List<EmployeeHoursDto> EmployeeHours { get; set; } = new();
    }
}
