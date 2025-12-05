using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Vendors
{
    [Authorize(Roles = "CompanyAdmin,Admin")]
    public class CreateModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public Vendor Vendor { get; set; } = new();

        public CreateModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
                return Page();

            Vendor.CompanyId = user.CompanyId;
            Vendor.CreatedDate = DateTime.UtcNow;
            Vendor.IsActive = true;

            await _unitOfWork.Vendors.AddAsync(Vendor);
            await _unitOfWork.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
