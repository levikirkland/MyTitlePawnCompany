using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Pages.Admin
{
    [Authorize(Roles = "CompanyAdmin")]
    public class UsersIndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public List<ApplicationUser> Users { get; set; } = new();
        public List<Store> Stores { get; set; } = new();
        public List<IdentityRole> Roles { get; set; } = new();
        public Dictionary<string, string> UserRoles { get; set; } = new();

        public UsersIndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
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

            var allUsers = _userManager.Users.Where(u => u.CompanyId == currentUser.CompanyId).ToList();
            Users = allUsers;

            Stores = (await _unitOfWork.Stores.GetCompanyStoresAsync(currentUser.CompanyId)).ToList();
            Roles = _roleManager.Roles.ToList();

            foreach (var user in Users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                UserRoles[user.Id] = string.Join(", ", userRoles);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string id)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound();

            var userToReset = await _userManager.FindByIdAsync(id);
            if (userToReset == null || userToReset.CompanyId != currentUser.CompanyId)
                return Forbid();

            try
            {
                await _userManager.RemovePasswordAsync(userToReset);
                var tempPassword = GenerateTemporaryPassword();
                var result = await _userManager.AddPasswordAsync(userToReset, tempPassword);

                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = "Failed to reset password.";
                    return RedirectToPage();
                }

                TempData["SuccessMessage"] = $"Password reset for {userToReset.Email}! New temporary password: {tempPassword}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error resetting password: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound();

            var userToDelete = await _userManager.FindByIdAsync(id);
            if (userToDelete == null || userToDelete.CompanyId != currentUser.CompanyId)
                return Forbid();

            if (userToDelete.Id == currentUser.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToPage();
            }

            try
            {
                var storeUsers = (await _unitOfWork.StoreUsers.FindAsync(su => su.UserId == userToDelete.Id)).ToList();
                foreach (var storeUser in storeUsers)
                {
                    await _unitOfWork.StoreUsers.DeleteAsync(storeUser);
                }
                await _unitOfWork.SaveChangesAsync();

                var result = await _userManager.DeleteAsync(userToDelete);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                    return RedirectToPage();
                }

                TempData["SuccessMessage"] = $"User {userToDelete.Email} deleted successfully!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
                return RedirectToPage();
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
