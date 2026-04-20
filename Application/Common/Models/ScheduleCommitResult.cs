namespace Application.Common.Models
{
    public class ScheduleCommitResult
    {
        public int AssignedShiftsCreated { get; }
        public int OpenShiftsCreated { get; }
        public IReadOnlyList<Guid> CreatedShiftIds { get; }

        public ScheduleCommitResult(
            int assignedShiftsCreated,
            int openShiftsCreated,
            IReadOnlyList<Guid> createdShiftIds)
        {
            AssignedShiftsCreated = assignedShiftsCreated;
            OpenShiftsCreated = openShiftsCreated;
            CreatedShiftIds = createdShiftIds;
        }
    }
}
