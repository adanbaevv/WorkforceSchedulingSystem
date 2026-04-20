namespace API.Dtos.Schedule
{
    public class CommitScheduleDto
    {
        public List<CommitScheduleSlotDto> Slots { get; set; } = new();
    }
}
