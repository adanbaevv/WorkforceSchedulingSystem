using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Pages
{
    public class SwitchUserModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Index");

        public async Task<IActionResult> OnPostAsync(Guid employeeId)
        {
            HttpContext.Session.SetString("currentEmployeeId", employeeId.ToString());

            // Flush the session store now so that the redirected-to request sees the new viewer
            // on its very first read (otherwise the role-switch can appear to take two clicks).
            await HttpContext.Session.CommitAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)
                && Uri.TryCreate(referer, UriKind.Absolute, out var refUri)
                && string.Equals(refUri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(refUri.PathAndQuery);
            }

            return RedirectToPage("/Index");
        }
    }
}
