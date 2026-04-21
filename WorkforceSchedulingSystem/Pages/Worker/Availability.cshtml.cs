using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Worker
{
    public class AvailabilityModel : PageModel
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly ICurrentUserService _currentUserService;

        public AvailabilityModel(
            IAvailabilityService availabilityService,
            ICurrentUserService currentUserService)
        {
            _availabilityService = availabilityService;
            _currentUserService = currentUserService;
        }

        public IReadOnlyList<Availability> Availabilities { get; set; } = new List<Availability>();
        public string ViewerName { get; set; } = string.Empty;

        [BindProperty]
        public DayOfWeek NewDayOfWeek { get; set; } = DayOfWeek.Monday;

        [BindProperty]
        public TimeSpan NewStartTime { get; set; } = TimeSpan.FromHours(9);

        [BindProperty]
        public TimeSpan NewEndTime { get; set; } = TimeSpan.FromHours(17);

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            ViewerName = _currentUserService.CurrentUserName;
            var id = _currentUserService.CurrentUserId;
            if (id.HasValue)
            {
                Availabilities = await _availabilityService.GetByEmployeeAsync(id.Value, cancellationToken);
            }
        }

        public async Task<IActionResult> OnPostAddAsync(CancellationToken cancellationToken)
        {
            var id = _currentUserService.CurrentUserId;
            if (!id.HasValue)
            {
                TempData["Error"] = "No current user.";
                return RedirectToPage();
            }

            try
            {
                await _availabilityService.CreateAsync(id.Value, NewDayOfWeek, NewStartTime, NewEndTime, cancellationToken);
                TempData["Success"] = $"Added availability: {NewDayOfWeek} {NewStartTime:hh\\:mm}–{NewEndTime:hh\\:mm}.";
            }
            catch (ValidationException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException ex) { TempData["Error"] = ex.Message; }
            catch (ConflictException ex) { TempData["Error"] = ex.Message; }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid availabilityId, CancellationToken cancellationToken)
        {
            try
            {
                await _availabilityService.DeleteAsync(availabilityId, cancellationToken);
                TempData["Success"] = "Availability window removed.";
            }
            catch (NotFoundException ex) { TempData["Error"] = ex.Message; }

            return RedirectToPage();
        }
    }
}
