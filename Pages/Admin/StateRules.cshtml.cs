using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Admin
{
    [Authorize(Roles = "CompanyAdmin")]
    public class StateRulesModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public StateSpecialRule? Rule { get; set; }

        [BindProperty(SupportsGet = true)]
        public int StoreId { get; set; }

        public Store? CurrentStore { get; set; }
        public List<StateSpecialRule> Rules { get; set; } = new List<StateSpecialRule>();
        public List<Store> Stores { get; set; } = new List<Store>();

        // US States list
        private static readonly Dictionary<string, string> USStates = new()
        {
            { "AL", "Alabama" }, { "AK", "Alaska" }, { "AZ", "Arizona" }, { "AR", "Arkansas" },
            { "CA", "California" }, { "CO", "Colorado" }, { "CT", "Connecticut" }, { "DE", "Delaware" },
            { "FL", "Florida" }, { "GA", "Georgia" }, { "HI", "Hawaii" }, { "ID", "Idaho" },
            { "IL", "Illinois" }, { "IN", "Indiana" }, { "IA", "Iowa" }, { "KS", "Kansas" },
            { "KY", "Kentucky" }, { "LA", "Louisiana" }, { "ME", "Maine" }, { "MD", "Maryland" },
            { "MA", "Massachusetts" }, { "MI", "Michigan" }, { "MN", "Minnesota" }, { "MS", "Mississippi" },
            { "MO", "Missouri" }, { "MT", "Montana" }, { "NE", "Nebraska" }, { "NV", "Nevada" },
            { "NH", "New Hampshire" }, { "NJ", "New Jersey" }, { "NM", "New Mexico" }, { "NY", "New York" },
            { "NC", "North Carolina" }, { "ND", "North Dakota" }, { "OH", "Ohio" }, { "OK", "Oklahoma" },
            { "OR", "Oregon" }, { "PA", "Pennsylvania" }, { "RI", "Rhode Island" }, { "SC", "South Carolina" },
            { "SD", "South Dakota" }, { "TN", "Tennessee" }, { "TX", "Texas" }, { "UT", "Utah" },
            { "VT", "Vermont" }, { "VA", "Virginia" }, { "WA", "Washington" }, { "WV", "West Virginia" },
            { "WI", "Wisconsin" }, { "WY", "Wyoming" }
        };

        public StateRulesModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();

            if (StoreId == 0 && Stores.Count > 0)
                StoreId = Stores.First().Id;

            if (StoreId == 0)
                return NotFound();

            CurrentStore = await _unitOfWork.Stores.GetByIdAsync(StoreId);
            if (CurrentStore == null || CurrentStore.CompanyId != user.CompanyId)
                return Forbid();

            Rules = (await _unitOfWork.StateSpecialRules.GetStoreStateRulesAsync(StoreId)).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var store = await _unitOfWork.Stores.GetByIdAsync(StoreId);
            if (store == null || store.CompanyId != user.CompanyId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();
                CurrentStore = store;
                Rules = (await _unitOfWork.StateSpecialRules.GetStoreStateRulesAsync(StoreId)).ToList();
                return Page();
            }

            if (Rule == null)
            {
                ModelState.AddModelError("", "Rule data is required.");
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();
                CurrentStore = store;
                Rules = (await _unitOfWork.StateSpecialRules.GetStoreStateRulesAsync(StoreId)).ToList();
                return Page();
            }

            Rule.StoreId = StoreId;
            Rule.StateCode = Rule.StateCode.ToUpper().Trim();
            Rule.CreatedDate = DateTime.UtcNow;
            Rule.IsActive = true;

            // Validate state code
            if (!USStates.ContainsKey(Rule.StateCode))
            {
                ModelState.AddModelError("Rule.StateCode", $"Invalid state code: {Rule.StateCode}");
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();
                CurrentStore = store;
                Rules = (await _unitOfWork.StateSpecialRules.GetStoreStateRulesAsync(StoreId)).ToList();
                return Page();
            }

            Rule.StateName = USStates[Rule.StateCode];

            try
            {
                await _unitOfWork.StateSpecialRules.AddAsync(Rule);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"State rules for {Rule.StateName} created successfully!";
                return RedirectToPage(new { storeId = StoreId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving rule: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error saving rule: {ex.Message}");
                
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();
                CurrentStore = store;
                Rules = (await _unitOfWork.StateSpecialRules.GetStoreStateRulesAsync(StoreId)).ToList();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var rule = await _unitOfWork.StateSpecialRules.GetByIdAsync(id);
            if (rule == null || rule.StoreId != StoreId)
                return Forbid();

            var store = await _unitOfWork.Stores.GetByIdAsync(StoreId);
            if (store == null || store.CompanyId != user.CompanyId)
                return Forbid();

            var stateName = rule.StateName;
            await _unitOfWork.StateSpecialRules.DeleteAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"State rules for {stateName} deleted successfully!";
            return RedirectToPage(new { storeId = StoreId });
        }
    }
}
