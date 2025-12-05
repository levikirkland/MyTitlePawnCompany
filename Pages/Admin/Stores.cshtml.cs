using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Admin
{
    [Authorize(Roles = "CompanyAdmin")]
    public class StoresModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public Store? Store { get; set; }

        public List<Store> Stores { get; set; } = new List<Store>();
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public StoresModel(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
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
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();
                return Page();
            }

            if (Store == null)
                return BadRequest();

            Store.CompanyId = user.CompanyId;
            Store.CreatedDate = DateTime.UtcNow;
            Store.IsActive = true;

            await _unitOfWork.Stores.AddAsync(Store);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Store '{Store.Name}' created successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(user.CompanyId)).ToList();
                return Page();
            }

            if (Store == null)
                return BadRequest();

            var existingStore = await _unitOfWork.Stores.GetByIdAsync(Store.Id);
            if (existingStore == null || existingStore.CompanyId != user.CompanyId)
                return Forbid();

            existingStore.Name = Store.Name;
            existingStore.Address = Store.Address;
            existingStore.Phone = Store.Phone;
            existingStore.Email = Store.Email;
            existingStore.StoreCode = Store.StoreCode;
            existingStore.IsActive = Store.IsActive;

            await _unitOfWork.Stores.UpdateAsync(existingStore);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Store '{Store.Name}' updated successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var store = await _unitOfWork.Stores.GetByIdAsync(id);
            if (store == null || store.CompanyId != user.CompanyId)
                return Forbid();

            var storeName = store.Name;
            await _unitOfWork.Stores.DeleteAsync(store);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Store '{storeName}' deleted successfully!";
            return RedirectToPage();
        }
    }
}
