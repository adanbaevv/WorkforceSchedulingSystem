using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Shifts
{
    public class CreateModel : PageModel
    {
        private readonly IShiftService _shiftService;
        private readonly IEmployeeService _employeeService;

        public CreateModel(IShiftService shiftService, IEmployeeService employeeService)
        {
            _shiftService = shiftService;
            _employeeService = employeeService;
        }

        [BindProperty]
        public InputFormModel Input { get; set; } = new();

        public IReadOnlyList<Employee> Employees { get; set; } = new List<Employee>();

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            Employees = await _employeeService.GetAllAsync(cancellationToken);

            if (Input.Date == default)
            {
                Input.Date = DateOnly.FromDateTime(DateTime.Today);
            }
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            try
            {
                var employeeId = Input.AssignedEmployeeId == Guid.Empty ? (Guid?)null : Input.AssignedEmployeeId;
                await _shiftService.CreateAsync(Input.Date, Input.StartTime, Input.EndTime, employeeId, cancellationToken);
                TempData["Success"] = employeeId.HasValue
                    ? "Shift created and assigned."
                    : "Open shift created — available for pickup.";
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

            Employees = await _employeeService.GetAllAsync(cancellationToken);
            return Page();
        }

        public class InputFormModel
        {
            public DateOnly Date { get; set; }
            public TimeSpan StartTime { get; set; } = new(9, 0, 0);
            public TimeSpan EndTime { get; set; } = new(17, 0, 0);
            public Guid AssignedEmployeeId { get; set; }
        }
    }
}
