using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Payments
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; } = string.Empty;

        public List<TitlePawn> SearchResults { get; set; } = new List<TitlePawn>();
        public string SearchType { get; set; } = string.Empty;
        public bool HasSearched { get; set; } = false;

        public IndexModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                HasSearched = true;

                // Get all active loans for the company
                var allLoans = (await _unitOfWork.TitlePawns.GetCompanyTitlePawnsAsync(user.CompanyId))
                    .Where(tp => tp.Status == "Active")
                    .ToList();

                // Search by last name
                var byLastName = allLoans
                    .Where(tp => tp.Vehicle?.Customer?.LastName != null && 
                                 tp.Vehicle.Customer.LastName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Search by phone number
                var byPhoneNumber = allLoans
                    .Where(tp => tp.Vehicle?.Customer?.PhoneNumber != null && 
                                 tp.Vehicle.Customer.PhoneNumber.Contains(SearchQuery))
                    .ToList();

                // Search by account number (Title Pawn ID)
                var byAccountNumber = allLoans
                    .Where(tp => tp.Id.ToString().Contains(SearchQuery))
                    .ToList();

                // Combine results (remove duplicates)
                SearchResults = byLastName
                    .Concat(byPhoneNumber)
                    .Concat(byAccountNumber)
                    .DistinctBy(tp => tp.Id)
                    .OrderByDescending(tp => tp.Id)
                    .ToList();

                // Determine search type for display
                if (int.TryParse(SearchQuery, out _))
                    SearchType = "Account Number";
                else if (SearchQuery.Contains("-") || SearchQuery.Length == 10)
                    SearchType = "Phone Number";
                else
                    SearchType = "Last Name";
            }

            return Page();
        }
    }
}
