using Application.Common.Interfaces;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IShiftService _shiftService;
        private readonly IShiftRequestService _shiftRequestService;
        private readonly ITimeEntryService _timeEntryService;
        private readonly ICurrentUserService _currentUserService;

        public IndexModel(
            IEmployeeService employeeService,
            IShiftService shiftService,
            IShiftRequestService shiftRequestService,
            ITimeEntryService timeEntryService,
            ICurrentUserService currentUserService)
        {
            _employeeService = employeeService;
            _shiftService = shiftService;
            _shiftRequestService = shiftRequestService;
            _timeEntryService = timeEntryService;
            _currentUserService = currentUserService;
        }

        public int EmployeeCount { get; set; }
        public int ShiftsThisWeekCount { get; set; }
        public int PendingRequestsCount { get; set; }
        public int CurrentlyClockedInCount { get; set; }
        public IReadOnlyList<RecentShiftItem> RecentShifts { get; set; } = new List<RecentShiftItem>();
        public string Greeting { get; set; } = "Hello";
        public string GreetedName { get; set; } = "there";
        public string TodayPretty { get; set; } = string.Empty;

        public WeekSummary ThisWeek { get; set; } = new();
        public WeekSummary NextWeek { get; set; } = new();
        public WeekSummary? Week3 { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowWeek3 { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            // Workers don't see the manager dashboard — route them to their own schedule.
            if (_currentUserService.CurrentUserRole == "Worker")
            {
                return RedirectToPage("/Worker/Schedule");
            }

            var now = DateTime.Now;
            Greeting = now.Hour < 12 ? "Good morning" : now.Hour < 18 ? "Good afternoon" : "Good evening";
            TodayPretty = now.ToString("dddd, MMMM d, yyyy");

            var employees = await _employeeService.GetAllAsync(cancellationToken);
            EmployeeCount = employees.Count;
            var employeeById = employees.ToDictionary(employee => employee.Id);

            GreetedName = _currentUserService.CurrentUserName.Split(' ').FirstOrDefault()
                          ?? _currentUserService.CurrentUserName;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var daysSinceMonday = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var thisMonday = today.AddDays(-daysSinceMonday);
            var nextMonday = thisMonday.AddDays(7);
            var week3Monday = thisMonday.AddDays(14);

            ThisWeek = await BuildWeekSummaryAsync(thisMonday, "This Week", cancellationToken);
            NextWeek = await BuildWeekSummaryAsync(nextMonday, "Next Week", cancellationToken);
            if (ShowWeek3)
            {
                Week3 = await BuildWeekSummaryAsync(week3Monday, "Week After Next", cancellationToken);
            }

            ShiftsThisWeekCount = ThisWeek.TotalShiftCount;

            var pending = await _shiftRequestService.GetPendingAsync(cancellationToken);
            PendingRequestsCount = pending.Count;

            var activeCount = 0;
            foreach (var employee in employees)
            {
                var active = await _timeEntryService.GetActiveByEmployeeAsync(employee.Id, cancellationToken);
                if (active != null)
                {
                    activeCount++;
                }
            }
            CurrentlyClockedInCount = activeCount;

            var allShifts = await _shiftService.GetAllAsync(cancellationToken);
            RecentShifts = allShifts
                .OrderByDescending(shift => shift.CreatedAt)
                .Take(5)
                .Select(shift => new RecentShiftItem
                {
                    Date = shift.Date,
                    StartTime = shift.StartTime,
                    EndTime = shift.EndTime,
                    EmployeeName = shift.AssignedEmployeeId.HasValue
                                   && employeeById.TryGetValue(shift.AssignedEmployeeId.Value, out var emp)
                        ? emp.FullName
                        : "Open"
                })
                .ToList();

            return Page();
        }

        private async Task<WeekSummary> BuildWeekSummaryAsync(DateOnly weekStart, string label, CancellationToken cancellationToken)
        {
            var weekEnd = weekStart.AddDays(6);
            var shifts = await _shiftService.GetByDateRangeAsync(weekStart, weekEnd, cancellationToken);
            var days = new List<DaySummary>();
            for (var i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                var daily = shifts.Where(shift => shift.Date == date).ToList();
                days.Add(new DaySummary
                {
                    Date = date,
                    AssignedCount = daily.Count(shift => shift.Status == ShiftStatus.Assigned),
                    OpenCount = daily.Count(shift => shift.Status == ShiftStatus.OpenForPickup)
                });
            }

            return new WeekSummary
            {
                Label = label,
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                Days = days,
                TotalShiftCount = shifts.Count
            };
        }

        public class WeekSummary
        {
            public string Label { get; set; } = string.Empty;
            public DateOnly WeekStart { get; set; }
            public DateOnly WeekEnd { get; set; }
            public List<DaySummary> Days { get; set; } = new();
            public int TotalShiftCount { get; set; }
        }

        public class DaySummary
        {
            public DateOnly Date { get; set; }
            public int AssignedCount { get; set; }
            public int OpenCount { get; set; }
        }

        public class RecentShiftItem
        {
            public DateOnly Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public string EmployeeName { get; set; } = string.Empty;
        }
    }
}
