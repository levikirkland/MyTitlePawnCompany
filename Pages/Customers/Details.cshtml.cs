using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.Customers
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ICustomerService _customerService;
        private readonly IVehicleService _vehicleService;

        public Customer? Customer { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new();

        public DetailsModel(ICustomerService customerService, IVehicleService vehicleService)
        {
            _customerService = customerService;
            _vehicleService = vehicleService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Customer = await _customerService.GetCustomerAsync(id.Value);
            if (Customer == null)
                return NotFound();

            Vehicles = await _vehicleService.GetCustomerVehiclesAsync(id.Value);
            return Page();
        }
    }
}
