using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Worker
{
    public class ClockModel : PageModel
    {
        private readonly ITimeEntryService _timeEntryService;
        private readonly IShiftService _shiftService;
        private readonly ICurrentUserService _currentUserService;

        public ClockModel(
            ITimeEntryService timeEntryService,
            IShiftService shiftService,
            ICurrentUserService currentUserService)
        {
            _timeEntryService = timeEntryService;
            _shiftService = shiftService;
            _currentUserService = currentUserService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid? ShiftId { get; set; }

        public TimeEntry? ActiveEntry { get; set; }
        public Shift? TargetShift { get; set; }
        public IReadOnlyList<TimeEntry> TodayEntries { get; set; } = new List<TimeEntry>();
        public string ViewerName { get; set; } = string.Empty;

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            await LoadAsync(cancellationToken);
        }

        public async Task<IActionResult> OnPostClockInAsync(CancellationToken cancellationToken)
        {
            var id = _currentUserService.CurrentUserId;
            if (!id.HasValue) { TempData["Error"] = "No current user."; return RedirectToPage(); }

            try
            {
                await _timeEntryService.ClockInAsync(id.Value, ShiftId, cancellationToken);
                TempData["Success"] = "Clocked in.";
            }
            catch (ValidationException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException ex) { TempData["Error"] = ex.Message; }
            catch (ConflictException ex) { TempData["Error"] = ex.Message; }

            return RedirectToPage(new { shiftId = ShiftId });
        }

        public async Task<IActionResult> OnPostClockOutAsync(Guid timeEntryId, CancellationToken cancellationToken)
        {
            try
            {
                await _timeEntryService.ClockOutAsync(timeEntryId, notes: null, cancellationToken);
                TempData["Success"] = "Clocked out.";
            }
            catch (ValidationException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException ex) { TempData["Error"] = ex.Message; }
            catch (ConflictException ex) { TempData["Error"] = ex.Message; }

            return RedirectToPage();
        }

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            ViewerName = _currentUserService.CurrentUserName;
            var id = _currentUserService.CurrentUserId;
            if (!id.HasValue) return;

            ActiveEntry = await _timeEntryService.GetActiveByEmployeeAsync(id.Value, cancellationToken);

            if (ShiftId.HasValue)
            {
                try { TargetShift = await _shiftService.GetByIdAsync(ShiftId.Value, cancellationToken); }
                catch (NotFoundException) { TargetShift = null; }
            }

            var history = await _timeEntryService.GetHistoryByEmployeeAsync(id.Value, cancellationToken);
            var today = DateTime.Today;
            TodayEntries = history
                .Where(entry => entry.ClockInAt.ToLocalTime().Date == today)
                .OrderBy(entry => entry.ClockInAt)
                .ToList();
        }
    }
}
