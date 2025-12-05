using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.Vehicles
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IVehicleService _vehicleService;

        public Vehicle? Vehicle { get; set; }
        public List<TitlePawn> TitlePawns { get; set; } = new();

        public DetailsModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Vehicle = await _vehicleService.GetVehicleAsync(id.Value);
            if (Vehicle == null)
                return NotFound();

            TitlePawns = Vehicle.TitlePawns?.ToList() ?? new();
            return Page();
        }
    }
}
