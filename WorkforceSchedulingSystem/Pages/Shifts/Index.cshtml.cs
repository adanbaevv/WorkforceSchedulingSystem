using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Shifts
{
    public class IndexModel : PageModel
    {
        private readonly IShiftService _shiftService;
        private readonly IEmployeeService _employeeService;

        public IndexModel(IShiftService shiftService, IEmployeeService employeeService)
        {
            _shiftService = shiftService;
            _employeeService = employeeService;
        }

        [BindProperty(SupportsGet = true)]
        public DateOnly WeekStartDate { get; set; }

        public DateOnly WeekEndDate { get; set; }
        public DateOnly PreviousWeekStart { get; set; }
        public DateOnly NextWeekStart { get; set; }
        public DateOnly NextWeekEnd { get; set; }
        public IReadOnlyList<Shift> Shifts { get; set; } = new List<Shift>();
        public IReadOnlyList<Shift> NextWeekShifts { get; set; } = new List<Shift>();
        public Dictionary<Guid, string> EmployeeNames { get; set; } = new();

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            if (WeekStartDate == default)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var daysSinceMonday = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                WeekStartDate = today.AddDays(-daysSinceMonday);
            }

            WeekEndDate = WeekStartDate.AddDays(6);
            PreviousWeekStart = WeekStartDate.AddDays(-7);
            NextWeekStart = WeekStartDate.AddDays(7);
            NextWeekEnd = NextWeekStart.AddDays(6);

            var employees = await _employeeService.GetAllAsync(cancellationToken);
            EmployeeNames = employees.ToDictionary(employee => employee.Id, employee => employee.FullName);

            Shifts = await _shiftService.GetByDateRangeAsync(WeekStartDate, WeekEndDate, cancellationToken);

            if (Shifts.Count == 0)
            {
                NextWeekShifts = await _shiftService.GetByDateRangeAsync(NextWeekStart, NextWeekEnd, cancellationToken);
            }
        }

        public string EmployeeNameOf(Guid? id)
        {
            if (!id.HasValue)
            {
                return "Open";
            }

            return EmployeeNames.TryGetValue(id.Value, out var name) ? name : "(unknown)";
        }
    }
}
