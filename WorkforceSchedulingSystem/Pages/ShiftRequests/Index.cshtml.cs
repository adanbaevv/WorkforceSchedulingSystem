using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.ShiftRequests
{
    public class IndexModel : PageModel
    {
        private readonly IShiftRequestService _shiftRequestService;
        private readonly IShiftService _shiftService;
        private readonly IEmployeeService _employeeService;

        public IndexModel(
            IShiftRequestService shiftRequestService,
            IShiftService shiftService,
            IEmployeeService employeeService)
        {
            _shiftRequestService = shiftRequestService;
            _shiftService = shiftService;
            _employeeService = employeeService;
        }

        public IReadOnlyList<ShiftRequest> PendingRequests { get; set; } = new List<ShiftRequest>();
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public Dictionary<Guid, Shift> ShiftLookup { get; set; } = new();
        public Dictionary<Guid, string> EmployeeNames { get; set; } = new();

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            var allRequests = await _shiftRequestService.GetAllAsync(cancellationToken);
            PendingRequests = allRequests
                .Where(request => request.Status == Domain.Enums.ShiftRequestStatus.Pending)
                .ToList();
            ApprovedCount = allRequests.Count(request => request.Status == Domain.Enums.ShiftRequestStatus.Approved);
            RejectedCount = allRequests.Count(request => request.Status == Domain.Enums.ShiftRequestStatus.Rejected);

            var employees = await _employeeService.GetAllAsync(cancellationToken);
            EmployeeNames = employees.ToDictionary(employee => employee.Id, employee => employee.FullName);

            var referencedShiftIds = PendingRequests.Select(request => request.ShiftId).Distinct().ToList();
            foreach (var shiftId in referencedShiftIds)
            {
                try
                {
                    var shift = await _shiftService.GetByIdAsync(shiftId, cancellationToken);
                    ShiftLookup[shiftId] = shift;
                }
                catch (NotFoundException)
                {
                    // shift gone; row will show a placeholder
                }
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _shiftRequestService.ApproveRequestAsync(id, cancellationToken);
                TempData["Success"] = "Request approved.";
            }
            catch (NotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (ConflictException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (ValidationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _shiftRequestService.RejectRequestAsync(id, cancellationToken);
                TempData["Success"] = "Request rejected.";
            }
            catch (NotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (ConflictException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (ValidationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage();
        }

        public string EmployeeNameOf(Guid id)
            => EmployeeNames.TryGetValue(id, out var name) ? name : "(unknown)";
    }
}
