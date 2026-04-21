using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Worker
{
    public class RequestsModel : PageModel
    {
        private readonly IShiftRequestService _shiftRequestService;
        private readonly IShiftService _shiftService;
        private readonly ICurrentUserService _currentUserService;

        public RequestsModel(
            IShiftRequestService shiftRequestService,
            IShiftService shiftService,
            ICurrentUserService currentUserService)
        {
            _shiftRequestService = shiftRequestService;
            _shiftService = shiftService;
            _currentUserService = currentUserService;
        }

        public IReadOnlyList<ShiftRequest> MyRequests { get; set; } = new List<ShiftRequest>();
        public IReadOnlyList<Shift> MyFutureAssignedShifts { get; set; } = new List<Shift>();
        public IReadOnlyList<Shift> OpenShifts { get; set; } = new List<Shift>();
        public Dictionary<Guid, Shift> ShiftLookup { get; set; } = new();

        [BindProperty]
        public Guid DropShiftId { get; set; }

        [BindProperty]
        public string? DropReason { get; set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            await LoadAsync(cancellationToken);
        }

        public async Task<IActionResult> OnPostDropAsync(CancellationToken cancellationToken)
        {
            var id = _currentUserService.CurrentUserId;
            if (!id.HasValue)
            {
                TempData["Error"] = "No current user.";
                return RedirectToPage();
            }

            if (DropShiftId == Guid.Empty)
            {
                TempData["Error"] = "Please choose a shift to drop.";
                return RedirectToPage();
            }

            try
            {
                await _shiftRequestService.CreateAsync(id.Value, DropShiftId, ShiftRequestType.Drop, DropReason, cancellationToken);
                TempData["Success"] = "Drop request submitted. Your manager will review it.";
            }
            catch (ValidationException ex) { TempData["Error"] = ex.Message; }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostPickupAsync(Guid shiftId, string? reason, CancellationToken cancellationToken)
        {
            var id = _currentUserService.CurrentUserId;
            if (!id.HasValue)
            {
                TempData["Error"] = "No current user.";
                return RedirectToPage();
            }

            try
            {
                await _shiftRequestService.CreateAsync(id.Value, shiftId, ShiftRequestType.Pickup, reason, cancellationToken);
                TempData["Success"] = "Pickup request submitted. Your manager will review it.";
            }
            catch (ValidationException ex) { TempData["Error"] = ex.Message; }

            return RedirectToPage();
        }

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            var id = _currentUserService.CurrentUserId;
            if (!id.HasValue) return;

            MyRequests = await _shiftRequestService.GetByEmployeeAsync(id.Value, cancellationToken);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var mine = await _shiftService.GetByEmployeeAsync(id.Value, cancellationToken);
            MyFutureAssignedShifts = mine
                .Where(shift => shift.Date >= today && shift.Status == ShiftStatus.Assigned)
                .OrderBy(shift => shift.Date)
                .ThenBy(shift => shift.StartTime)
                .ToList();

            OpenShifts = (await _shiftService.GetOpenShiftsAsync(cancellationToken))
                .Where(shift => shift.Date >= today)
                .OrderBy(shift => shift.Date)
                .ThenBy(shift => shift.StartTime)
                .ToList();

            var shiftIds = MyRequests.Select(r => r.ShiftId).Distinct();
            foreach (var sid in shiftIds)
            {
                try
                {
                    ShiftLookup[sid] = await _shiftService.GetByIdAsync(sid, cancellationToken);
                }
                catch (NotFoundException) { }
            }
        }
    }
}
