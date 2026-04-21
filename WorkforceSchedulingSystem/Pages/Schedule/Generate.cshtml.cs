using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Schedule
{
    public class GenerateModel : PageModel
    {
        private readonly IScheduleService _scheduleService;

        public GenerateModel(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [BindProperty]
        public InputFormModel Input { get; set; } = new();

        public ScheduleProposal? Proposal { get; set; }

        public void OnGet()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var daysSinceMonday = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var thisMonday = today.AddDays(-daysSinceMonday);
            var nextMonday = thisMonday.AddDays(7);

            Input = new InputFormModel
            {
                WeekStartDate = nextMonday,
                Slots = new List<SlotInput>
                {
                    new() { Date = nextMonday,               StartTime = new TimeSpan(9, 0, 0),  EndTime = new TimeSpan(17, 0, 0) },
                    new() { Date = nextMonday.AddDays(1),    StartTime = new TimeSpan(9, 0, 0),  EndTime = new TimeSpan(17, 0, 0) },
                    new() { Date = nextMonday.AddDays(2),    StartTime = new TimeSpan(9, 0, 0),  EndTime = new TimeSpan(17, 0, 0) },
                    new() { Date = nextMonday.AddDays(3),    StartTime = new TimeSpan(9, 0, 0),  EndTime = new TimeSpan(17, 0, 0) },
                    new() { Date = nextMonday.AddDays(4),    StartTime = new TimeSpan(9, 0, 0),  EndTime = new TimeSpan(17, 0, 0) },
                    new() { Date = nextMonday.AddDays(5),    StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                    new() { Date = nextMonday.AddDays(6),    StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0) },
                }
            };
        }

        public async Task<IActionResult> OnPostGenerateAsync(CancellationToken cancellationToken)
        {
            try
            {
                var slots = Input.Slots
                    .Select(slot => new ShiftSlot(slot.Date, slot.StartTime, slot.EndTime))
                    .ToList();

                Proposal = await _scheduleService.GenerateAsync(Input.WeekStartDate, slots, cancellationToken);
            }
            catch (ValidationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCommitAsync(List<CommitSlotInput> commitSlots, CancellationToken cancellationToken)
        {
            try
            {
                var slots = commitSlots
                    .Select(slot => new CommitScheduleSlot(slot.Date, slot.StartTime, slot.EndTime, slot.AssignedEmployeeId))
                    .ToList();

                var result = await _scheduleService.CommitAsync(slots, cancellationToken);
                TempData["Success"] =
                    $"Committed {result.AssignedShiftsCreated} assigned shift(s) and {result.OpenShiftsCreated} open shift(s).";

                return RedirectToPage("/Shifts/Index");
            }
            catch (ValidationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (NotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (ConflictException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return Page();
        }

        public class InputFormModel
        {
            public DateOnly WeekStartDate { get; set; }
            public List<SlotInput> Slots { get; set; } = new();
        }

        public class SlotInput
        {
            public DateOnly Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }

        public class CommitSlotInput
        {
            public DateOnly Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public Guid? AssignedEmployeeId { get; set; }
        }
    }
}
