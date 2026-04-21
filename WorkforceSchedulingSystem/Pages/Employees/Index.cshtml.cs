using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Employees
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public IndexModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IReadOnlyList<Employee> Employees { get; set; } = new List<Employee>();

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            Employees = await _employeeService.GetAllAsync(cancellationToken);
        }

        public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _employeeService.DeleteAsync(id, cancellationToken);
                TempData["Success"] = "Employee deactivated.";
            }
            catch (NotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage();
        }
    }
}
