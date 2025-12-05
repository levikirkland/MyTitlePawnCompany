using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.TitlePawns
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ITitlePawnService _titlePawnService;
        private readonly IFeeService _feeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TitlePawn? TitlePawn { get; set; }
        public List<Payment> Payments { get; set; } = new();
        public List<Fee> Fees { get; set; } = new();
        public decimal AccumulatedLateFees { get; set; }
        public int BusinessDaysLate { get; set; }

        [BindProperty]
        public string FeeType { get; set; } = string.Empty;

        [BindProperty]
        public decimal FeeAmount { get; set; }

        [BindProperty]
        public string? FeeDescription { get; set; }

        public DetailsModel(ITitlePawnService titlePawnService, IFeeService feeService, UserManager<ApplicationUser> userManager)
        {
            _titlePawnService = titlePawnService;
            _feeService = feeService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            TitlePawn = await _titlePawnService.GetTitlePawnAsync(id.Value);
            if (TitlePawn == null)
                return NotFound();

            Payments = TitlePawn.Payments?.ToList() ?? new();
            Fees = (await _feeService.GetTitlePawnFeesAsync(id.Value)).ToList();
            
            // Calculate accumulated late fees if overdue
            if (TitlePawn.Status == "Active" && DateTime.UtcNow > TitlePawn.LoanMaturityDate)
            {
                AccumulatedLateFees = TitlePawn.CalculateLateFees(DateTime.UtcNow);
                BusinessDaysLate = TitlePawn.CalculateBusinessDaysLate(DateTime.UtcNow);
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostAddFeeAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                TitlePawn = await _titlePawnService.GetTitlePawnAsync(id);
                Payments = TitlePawn?.Payments?.ToList() ?? new();
                Fees = (await _feeService.GetTitlePawnFeesAsync(id)).ToList();
                return Page();
            }

            var success = await _feeService.AddFeeAsync(id, FeeType, FeeAmount, FeeDescription, user.CompanyId);
            if (success)
            {
                TempData["SuccessMessage"] = $"Fee of ${FeeAmount:N2} added successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add fee.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostWaiveFeeAsync(int id, int feeId, string waiveReason)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var userName = $"{user.FirstName} {user.LastName}";
            var success = await _feeService.WaiveFeeAsync(feeId, waiveReason, userName, user.CompanyId);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Fee waived successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to waive fee.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostApplyLateFeeAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var success = await _feeService.ApplyLateFeeAsync(id, user.CompanyId);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Late fee applied successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Late fee could not be applied (loan not overdue or already applied today).";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostMarkContractSignedAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            TitlePawn = await _titlePawnService.GetTitlePawnAsync(id);
            if (TitlePawn == null || TitlePawn.CompanyId != user.CompanyId)
                return NotFound();

            // Mark contract as signed
            TitlePawn.ContractSigned = true;
            TitlePawn.ContractSignedDate = DateTime.UtcNow;
            await _titlePawnService.UpdateTitlePawnAsync(TitlePawn);

            TempData["SuccessMessage"] = "Contract marked as signed successfully.";
            return RedirectToPage(new { id });
        }
    }
}
