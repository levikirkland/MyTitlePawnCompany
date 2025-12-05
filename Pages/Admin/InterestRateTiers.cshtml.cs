using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Admin
{
    [Authorize(Roles = "CompanyAdmin")]
    public class InterestRateTiersModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public InterestRateTier? Tier { get; set; }

        [BindProperty(SupportsGet = true)]
        public int StoreId { get; set; }

        public Store? CurrentStore { get; set; }
        public List<InterestRateTier> Tiers { get; set; } = new List<InterestRateTier>();
        public List<Store> Stores { get; set; } = new List<Store>();

        public InterestRateTiersModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
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

            Tiers = (await _unitOfWork.InterestRateTiers.GetStoreRateTiersAsync(StoreId)).ToList();
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

            if (!ModelState.IsValid || Tier == null)
            {
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();
                CurrentStore = store;
                Tiers = (await _unitOfWork.InterestRateTiers.GetStoreRateTiersAsync(StoreId)).ToList();
                return Page();
            }

            Tier.StoreId = StoreId;
            Tier.CreatedDate = DateTime.UtcNow;
            Tier.IsActive = true;

            await _unitOfWork.InterestRateTiers.AddAsync(Tier);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Interest tier '{Tier.TierName}' created successfully!";
            return RedirectToPage(new { storeId = StoreId });
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var store = await _unitOfWork.Stores.GetByIdAsync(StoreId);
            if (store == null || store.CompanyId != user.CompanyId)
                return Forbid();

            if (!ModelState.IsValid || Tier == null)
            {
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();
                CurrentStore = store;
                Tiers = (await _unitOfWork.InterestRateTiers.GetStoreRateTiersAsync(StoreId)).ToList();
                return Page();
            }

            var existingTier = await _unitOfWork.InterestRateTiers.GetByIdAsync(Tier.Id);
            if (existingTier == null || existingTier.StoreId != StoreId)
                return Forbid();

            existingTier.TierName = Tier.TierName;
            existingTier.MinimumPrincipal = Tier.MinimumPrincipal;
            existingTier.MaximumPrincipal = Tier.MaximumPrincipal;
            existingTier.InterestRate = Tier.InterestRate;
            existingTier.Description = Tier.Description;
            existingTier.DisplayOrder = Tier.DisplayOrder;
            existingTier.IsActive = Tier.IsActive;
            existingTier.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.InterestRateTiers.UpdateAsync(existingTier);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Interest tier '{Tier.TierName}' updated successfully!";
            return RedirectToPage(new { storeId = StoreId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var tier = await _unitOfWork.InterestRateTiers.GetByIdAsync(id);
            if (tier == null || tier.StoreId != StoreId)
                return Forbid();

            var store = await _unitOfWork.Stores.GetByIdAsync(StoreId);
            if (store == null || store.CompanyId != user.CompanyId)
                return Forbid();

            var tierName = tier.TierName;
            await _unitOfWork.InterestRateTiers.DeleteAsync(tier);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Interest tier '{tierName}' deleted successfully!";
            return RedirectToPage(new { storeId = StoreId });
        }
    }
}
