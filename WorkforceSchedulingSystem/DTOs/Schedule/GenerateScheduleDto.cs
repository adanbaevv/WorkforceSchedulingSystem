namespace API.Dtos.Schedule
{
    public class GenerateScheduleDto
    {
        public DateOnly WeekStartDate { get; set; }
        public List<ShiftSlotDto> Slots { get; set; } = new();
    }
}
