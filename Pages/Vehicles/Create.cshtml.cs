using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.Vehicles
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public Vehicle Vehicle { get; set; } = new();

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public CreateModel(IVehicleService vehicleService, ICustomerService customerService, UserManager<ApplicationUser> userManager)
        {
            _vehicleService = vehicleService;
            _customerService = customerService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int customerId)
        {
            CustomerId = customerId;
            Customer = await _customerService.GetCustomerAsync(customerId);
            if (Customer == null)
                return NotFound();

            Vehicle.CustomerId = customerId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CustomerId = Vehicle.CustomerId;
                Customer = await _customerService.GetCustomerAsync(Vehicle.CustomerId);
                return Page();
            }

            await _vehicleService.CreateVehicleAsync(Vehicle);
            return RedirectToPage("/Customers/Details", new { id = Vehicle.CustomerId });
        }
    }
}
