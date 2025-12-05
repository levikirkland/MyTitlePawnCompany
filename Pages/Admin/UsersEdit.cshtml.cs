using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Admin
{
    [Authorize(Roles = "CompanyAdmin")]
    public class UsersEditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ApplicationUser? User { get; set; }

        [BindProperty]
        public string? SelectedRole { get; set; }

        [BindProperty]
        public int? DefaultStoreId { get; set; }

        public List<int> SelectedStores { get; set; } = new();
        public List<Store> Stores { get; set; } = new();
        public List<IdentityRole> Roles { get; set; } = new();

        public UsersEditModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound();

            User = await _userManager.FindByIdAsync(id);
            if (User == null || User.CompanyId != currentUser.CompanyId)
                return Forbid();

            Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
            Roles = _roleManager.Roles.ToList();

            var userRoles = await _userManager.GetRolesAsync(User);
            SelectedRole = userRoles.FirstOrDefault();

            var userStores = (await _unitOfWork.StoreUsers.FindAsync(su => su.UserId == User.Id)).ToList();
            SelectedStores = userStores.Select(s => s.StoreId).ToList();
            DefaultStoreId = userStores.FirstOrDefault(s => s.IsDefault)?.StoreId;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound();

            User = await _userManager.FindByIdAsync(id);
            if (User == null || User.CompanyId != currentUser.CompanyId)
                return Forbid();

            var storesParam = Request.Form["SelectedStores"];
            var selectedStores = storesParam.Select(s => int.Parse(s)).ToList();

            if (!ModelState.IsValid || selectedStores.Count == 0 || !DefaultStoreId.HasValue)
            {
                ModelState.AddModelError("", "Please fill all required fields and select at least one store.");
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
                Roles = _roleManager.Roles.ToList();
                SelectedStores = selectedStores;
                return Page();
            }

            try
            {
                var result = await _userManager.UpdateAsync(User);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
                    Roles = _roleManager.Roles.ToList();
                    SelectedStores = selectedStores;
                    return Page();
                }

                if (!string.IsNullOrEmpty(SelectedRole))
                {
                    var currentRoles = await _userManager.GetRolesAsync(User);
                    if (!currentRoles.Contains(SelectedRole))
                    {
                        await _userManager.RemoveFromRolesAsync(User, currentRoles);
                        await _userManager.AddToRoleAsync(User, SelectedRole);
                    }
                }

                var existingStoreUsers = (await _unitOfWork.StoreUsers.FindAsync(su => su.UserId == User.Id)).ToList();

                foreach (var storeUser in existingStoreUsers)
                {
                    if (!selectedStores.Contains(storeUser.StoreId))
                    {
                        await _unitOfWork.StoreUsers.DeleteAsync(storeUser);
                    }
                    else
                    {
                        storeUser.IsDefault = storeUser.StoreId == DefaultStoreId;
                        await _unitOfWork.StoreUsers.UpdateAsync(storeUser);
                    }
                }

                foreach (var storeId in selectedStores)
                {
                    if (!existingStoreUsers.Any(su => su.StoreId == storeId))
                    {
                        var storeUser = new StoreUser
                        {
                            UserId = User.Id,
                            StoreId = storeId,
                            IsDefault = storeId == DefaultStoreId,
                            CreatedDate = DateTime.UtcNow
                        };
                        await _unitOfWork.StoreUsers.AddAsync(storeUser);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User {User.Email} updated successfully!";
                return RedirectToPage("./UsersIndex");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating user: {ex.Message}");
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
                Roles = _roleManager.Roles.ToList();
                SelectedStores = selectedStores;
                return Page();
            }
        }
    }
}
