using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Employees
{
    public class AvailabilityModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityModel(
            IEmployeeService employeeService,
            IAvailabilityService availabilityService)
        {
            _employeeService = employeeService;
            _availabilityService = availabilityService;
        }

        public Employee? Employee { get; set; }
        public IReadOnlyList<Availability> Availabilities { get; set; } = new List<Availability>();

        [BindProperty]
        public DayOfWeek NewDayOfWeek { get; set; } = DayOfWeek.Monday;

        [BindProperty]
        public TimeSpan NewStartTime { get; set; } = TimeSpan.FromHours(9);

        [BindProperty]
        public TimeSpan NewEndTime { get; set; } = TimeSpan.FromHours(17);

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                Employee = await _employeeService.GetByIdAsync(id, cancellationToken);
                Availabilities = await _availabilityService.GetByEmployeeAsync(id, cancellationToken);
            }
            catch (NotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage("/Employees/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _availabilityService.CreateAsync(id, NewDayOfWeek, NewStartTime, NewEndTime, cancellationToken);
                TempData["Success"] = $"Added availability: {NewDayOfWeek} {NewStartTime:hh\\:mm}–{NewEndTime:hh\\:mm}.";
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

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id, Guid availabilityId, CancellationToken cancellationToken)
        {
            try
            {
                await _availabilityService.DeleteAsync(availabilityId, cancellationToken);
                TempData["Success"] = "Availability window removed.";
            }
            catch (NotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }
    }
}
