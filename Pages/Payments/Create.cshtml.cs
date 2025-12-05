using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.Payments
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ITitlePawnService _titlePawnService;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public Payment Payment { get; set; } = new();

        [BindProperty]
        public int TitlePawnId { get; set; }

        public TitlePawn? TitlePawn { get; set; }
        public decimal MaxPaymentAmount { get; set; }
        public decimal InterestDue { get; set; }
        public bool InterestAlreadyPaidThisMonth { get; set; }

        public CreateModel(ITitlePawnService titlePawnService, UserManager<ApplicationUser> userManager)
        {
            _titlePawnService = titlePawnService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int titlePawnId)
        {
            TitlePawnId = titlePawnId;
            TitlePawn = await _titlePawnService.GetTitlePawnAsync(titlePawnId);
            if (TitlePawn == null)
                return NotFound();

            Payment.TitlePawnId = titlePawnId;
            Payment.DueDate = TitlePawn.LoanMaturityDate;

            // Calculate maximum payment amount and interest due
            CalculatePaymentAmounts();

            return Page();
        }

        private void CalculatePaymentAmounts()
        {
            if (TitlePawn == null)
                return;

            // Check if interest was already paid this month
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var paymentsThisMonth = TitlePawn.Payments?
                .Where(p => p.PaymentDate.Month == currentMonth && p.PaymentDate.Year == currentYear)
                .ToList() ?? new List<Payment>();

            // Interest is due if no payments made this month, or only principal payments made
            InterestAlreadyPaidThisMonth = paymentsThisMonth.Any(p => 
                p.Amount >= TitlePawn.MonthlyPayment); // If any payment covers the minimum (interest)

            // Minimum payment is always the interest amount
            InterestDue = TitlePawn.TotalInterestCharged;
            
            // Maximum payment is principal + interest (if not paid) or principal only (if paid)
            if (InterestAlreadyPaidThisMonth)
            {
                // Interest already paid, max is just principal
                MaxPaymentAmount = TitlePawn.RemainingBalance + InterestDue;
            }
            else
            {
                // Interest not paid yet, max includes principal + interest + any late fees
                var lateFees = TitlePawn.CalculateLateFees(DateTime.UtcNow);
                MaxPaymentAmount = TitlePawn.RemainingBalance + InterestDue + lateFees;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Load TitlePawn first before validation
            TitlePawn = await _titlePawnService.GetTitlePawnAsync(TitlePawnId);
            if (TitlePawn == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                CalculatePaymentAmounts();
                return Page();
            }

            // Validate payment amount
            CalculatePaymentAmounts();
            if (Payment.Amount < TitlePawn.TotalInterestCharged || Payment.Amount > MaxPaymentAmount)
            {
                ModelState.AddModelError("Payment.Amount", 
                    $"Payment must be between ${TitlePawn.TotalInterestCharged:N2} (minimum interest) and ${MaxPaymentAmount:N2} (maximum).");
                return Page();
            }

            Payment.CompanyId = user.CompanyId;
            var success = await _titlePawnService.ProcessPaymentAsync(
                TitlePawnId,
                Payment.Amount,
                Payment.PaymentType,
                Payment.PaymentMethod,
                user.CompanyId
            );

            if (!success)
                return NotFound();

            // Get the updated loan to check if a new contract should be generated
            TitlePawn = await _titlePawnService.GetTitlePawnAsync(TitlePawnId);
            if (TitlePawn != null && Payment.PaymentType != "Payoff" && TitlePawn.Status == "Active")
            {
                // Reset contract signature for new contract
                TitlePawn.ContractSigned = false;
                TitlePawn.ContractSignedDate = null;
                await _titlePawnService.UpdateTitlePawnAsync(TitlePawn);

                TempData["NewContractCreated"] = true;
                TempData["TitlePawnId"] = TitlePawnId;
            }

            return RedirectToPage("/TitlePawns/Details", new { id = TitlePawnId });
        }
    }
}
