using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.Customers
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public Customer Customer { get; set; } = new();

        public CreateModel(ICustomerService customerService, UserManager<ApplicationUser> userManager)
        {
            _customerService = customerService;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            Customer.CompanyId = user.CompanyId;
            await _customerService.CreateCustomerAsync(Customer);

            return RedirectToPage("./Index");
        }
    }
}
