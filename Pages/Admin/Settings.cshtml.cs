using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Admin
{
    [Authorize(Roles = "CompanyAdmin")]
    public class SettingsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public Company? Company { get; set; }

        public SettingsModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            Company = user.Company;
            if (Company == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
                return Page();

            Company = user.Company;
            if (Company == null)
                return NotFound();

            // Update company settings
            Company.TitleAndKeyFee = Company.TitleAndKeyFee;
            Company.Name = Company.Name;
            Company.Address = Company.Address;
            Company.Phone = Company.Phone;
            Company.Email = Company.Email;

            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Settings saved successfully!";
            return Page();
        }
    }
}
