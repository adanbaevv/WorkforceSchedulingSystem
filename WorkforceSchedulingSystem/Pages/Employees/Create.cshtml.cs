using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages.Employees
{
    public class CreateModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public CreateModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [BindProperty]
        public InputFormModel Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _employeeService.CreateAsync(Input.FullName, Input.Email, Input.Role, cancellationToken);
                TempData["Success"] = $"Added {Input.FullName} to the team.";
                return RedirectToPage("/Employees/Index");
            }
            catch (ValidationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return Page();
        }

        public class InputFormModel
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public EmployeeRole Role { get; set; } = EmployeeRole.Worker;
        }
    }
}
