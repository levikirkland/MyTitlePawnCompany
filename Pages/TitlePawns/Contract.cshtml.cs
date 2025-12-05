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
    public class ContractModel : PageModel
    {
        private readonly ITitlePawnService _titlePawnService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TitlePawn? TitlePawn { get; set; }
        public Customer? Customer { get; set; }
        public Company? Company { get; set; }
        public Vehicle? Vehicle { get; set; }

        public ContractModel(ITitlePawnService titlePawnService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _titlePawnService = titlePawnService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                TitlePawn = await _titlePawnService.GetTitlePawnAsync(id);
                if (TitlePawn == null)
                    return NotFound("Title Pawn not found");

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound("User not found");

                // Verify the loan belongs to the user's company
                if (TitlePawn.CompanyId != user.CompanyId)
                    return Forbid();

                // Get vehicle from title pawn
                Vehicle = TitlePawn.Vehicle;
                if (Vehicle == null)
                    return NotFound("Vehicle not found");

                // Get customer from vehicle
                Customer = await _unitOfWork.Customers.GetByIdAsync(Vehicle.CustomerId);
                if (Customer == null)
                    return NotFound("Customer not found");

                // Get company - create a minimal company object if needed
                Company = new Company 
                { 
                    Name = user.Company?.Name ?? "Title Pawn Company",
                    Address = user.Company?.Address ?? "",
                    Phone = user.Company?.Phone ?? "",
                    Email = user.Company?.Email ?? ""
                };

                return Page();
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging
                return BadRequest($"Error loading contract: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostPrintAsync(int id)
        {
            // This will be handled by the browser's print functionality
            return await OnGetAsync(id);
        }
    }
}
