using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Admin
{
    [Authorize(Roles = "CompanyAdmin")]
    public class UsersCreateModel : PageModel
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

        public List<Store> Stores { get; set; } = new();
        public List<IdentityRole> Roles { get; set; } = new();

        public UsersCreateModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound();

            Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
            Roles = _roleManager.Roles.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound();

            var storesParam = Request.Form["SelectedStores"];
            var selectedStores = storesParam.Select(s => int.Parse(s)).ToList();

            if (!ModelState.IsValid || User == null || selectedStores.Count == 0 || !DefaultStoreId.HasValue)
            {
                ModelState.AddModelError("", "Please fill all required fields and select at least one store.");
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
                Roles = _roleManager.Roles.ToList();
                return Page();
            }

            var existingUser = await _userManager.FindByEmailAsync(User.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "A user with this email already exists.");
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
                Roles = _roleManager.Roles.ToList();
                return Page();
            }

            try
            {
                var newUser = new ApplicationUser
                {
                    UserName = User.Email,
                    Email = User.Email,
                    FirstName = User.FirstName,
                    LastName = User.LastName,
                    Phone = User.Phone,
                    Address = User.Address,
                    CompanyId = currentUser.CompanyId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var tempPassword = GenerateTemporaryPassword();
                var result = await _userManager.CreateAsync(newUser, tempPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
                    Roles = _roleManager.Roles.ToList();
                    return Page();
                }

                if (!string.IsNullOrEmpty(SelectedRole))
                {
                    await _userManager.AddToRoleAsync(newUser, SelectedRole);
                }

                foreach (var storeId in selectedStores)
                {
                    var storeUser = new StoreUser
                    {
                        UserId = newUser.Id,
                        StoreId = storeId,
                        IsDefault = storeId == DefaultStoreId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.StoreUsers.AddAsync(storeUser);
                }
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User {newUser.Email} created successfully! Temporary password: {tempPassword}";
                return RedirectToPage("./UsersIndex");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating user: {ex.Message}");
                Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
                Roles = _roleManager.Roles.ToList();
                return Page();
            }
        }

        private string GenerateTemporaryPassword()
        {
            var random = new Random();
            var password = new List<char>();
            password.Add((char)random.Next('A', 'Z' + 1));
            password.Add((char)random.Next('a', 'z' + 1));
            password.Add((char)random.Next('0', '9' + 1));
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            for (int i = 0; i < 5; i++)
            {
                password.Add(chars[random.Next(chars.Length)]);
            }
            var shuffled = password.OrderBy(_ => random.Next()).ToList();
            return new string(shuffled.ToArray());
        }
    }
}
