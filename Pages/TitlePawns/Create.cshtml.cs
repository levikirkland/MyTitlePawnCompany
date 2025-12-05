using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.TitlePawns
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITitlePawnService _titlePawnService;
        private readonly IVehicleService _vehicleService;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public TitlePawn TitlePawn { get; set; } = new();

        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new();

        public CreateModel(IUnitOfWork unitOfWork, ITitlePawnService titlePawnService, IVehicleService vehicleService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _titlePawnService = titlePawnService;
            _vehicleService = vehicleService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int? vehicleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            VehicleId = vehicleId;
            if (vehicleId.HasValue)
            {
                Vehicle = await _vehicleService.GetVehicleAsync(vehicleId.Value);
                if (Vehicle == null)
                    return NotFound();

                TitlePawn.VehicleId = vehicleId.Value;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            Console.WriteLine($"[CREATE] Starting loan creation for user: {user.Id}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[CREATE] ModelState invalid. Errors:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"  - {error.ErrorMessage}");
                }
                VehicleId = TitlePawn.VehicleId;
                Vehicle = await _vehicleService.GetVehicleAsync(TitlePawn.VehicleId);
                return Page();
            }

            try
            {
                TitlePawn.CompanyId = user.CompanyId;
                TitlePawn.InterestRate = 0m; // Will be set during approval

                Console.WriteLine($"[CREATE] Getting default store for user: {user.Id}");
                
                // Get user's default store
                var storeUser = await _unitOfWork.StoreUsers.GetDefaultStoreForUserAsync(user.Id);
                if (storeUser == null)
                {
                    Console.WriteLine($"[CREATE] No default store found for user: {user.Id}");
                    ModelState.AddModelError("", "You must be assigned to a store to create a loan.");
                    VehicleId = TitlePawn.VehicleId;
                    Vehicle = await _vehicleService.GetVehicleAsync(TitlePawn.VehicleId);
                    return Page();
                }

                TitlePawn.StoreId = storeUser.StoreId;
                Console.WriteLine($"[CREATE] Assigned StoreId: {TitlePawn.StoreId}");
                Console.WriteLine($"[CREATE] TitlePawn details - VehicleId: {TitlePawn.VehicleId}, Amount: {TitlePawn.LoanAmountRequested}, CompanyId: {TitlePawn.CompanyId}");

                await _titlePawnService.CreateTitlePawnAsync(TitlePawn);
                
                Console.WriteLine($"[CREATE] Loan created successfully with ID: {TitlePawn.Id}");
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CREATE] Exception: {ex.Message}");
                Console.WriteLine($"[CREATE] StackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error creating loan: {ex.Message}");
                VehicleId = TitlePawn.VehicleId;
                Vehicle = await _vehicleService.GetVehicleAsync(TitlePawn.VehicleId);
                return Page();
            }
        }
    }
}
