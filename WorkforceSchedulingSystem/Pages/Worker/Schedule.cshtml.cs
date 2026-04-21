using Application.Common.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Worker
{
    public class ScheduleModel : PageModel
    {
        private readonly IShiftService _shiftService;
        private readonly ITimeEntryService _timeEntryService;
        private readonly ICurrentUserService _currentUserService;

        public ScheduleModel(
            IShiftService shiftService,
            ITimeEntryService timeEntryService,
            ICurrentUserService currentUserService)
        {
            _shiftService = shiftService;
            _timeEntryService = timeEntryService;
            _currentUserService = currentUserService;
        }

        public IReadOnlyList<Shift> UpcomingShifts { get; set; } = new List<Shift>();
        public TimeEntry? ActiveEntry { get; set; }
        public DateOnly Today { get; set; }
        public string ViewerName { get; set; } = string.Empty;

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            Today = DateOnly.FromDateTime(DateTime.Today);
            ViewerName = _currentUserService.CurrentUserName;

            var employeeId = _currentUserService.CurrentUserId;
            if (!employeeId.HasValue)
            {
                return;
            }

            var all = await _shiftService.GetByEmployeeAsync(employeeId.Value, cancellationToken);
            UpcomingShifts = all
                .Where(shift => shift.Date >= Today)
                .OrderBy(shift => shift.Date)
                .ThenBy(shift => shift.StartTime)
                .ToList();

            ActiveEntry = await _timeEntryService.GetActiveByEmployeeAsync(employeeId.Value, cancellationToken);
        }

        public bool IsShiftToday(Shift shift) => shift.Date == Today;
    }
}
