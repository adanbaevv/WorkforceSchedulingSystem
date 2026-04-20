namespace Application.Common.Models
{
    public class ScheduleProposal
    {
        public DateOnly WeekStartDate { get; }
        public DateOnly WeekEndDate { get; }
        public IReadOnlyList<ScheduledAssignment> Assignments { get; }
        public IReadOnlyList<UnassignedSlot> UnassignedSlots { get; }
        public IReadOnlyList<EmployeeScheduledHours> EmployeeHours { get; }

        public ScheduleProposal(
            DateOnly weekStartDate,
            DateOnly weekEndDate,
            IReadOnlyList<ScheduledAssignment> assignments,
            IReadOnlyList<UnassignedSlot> unassignedSlots,
            IReadOnlyList<EmployeeScheduledHours> employeeHours)
        {
            WeekStartDate = weekStartDate;
            WeekEndDate = weekEndDate;
            Assignments = assignments;
            UnassignedSlots = unassignedSlots;
            EmployeeHours = employeeHours;
        }
    }
}
