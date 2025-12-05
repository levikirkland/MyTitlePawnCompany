using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.TitlePawns
{
    [Authorize]
    public class PaymentScheduleModel : PageModel
    {
        private readonly ITitlePawnService _titlePawnService;
        private readonly IPaymentScheduleService _paymentScheduleService;

        public TitlePawn? TitlePawn { get; set; }
        public List<PaymentScheduleItem> Schedule { get; set; } = new();

        public PaymentScheduleModel(ITitlePawnService titlePawnService, IPaymentScheduleService paymentScheduleService)
        {
            _titlePawnService = titlePawnService;
            _paymentScheduleService = paymentScheduleService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            TitlePawn = await _titlePawnService.GetTitlePawnAsync(id);
            if (TitlePawn == null)
                return NotFound();

            // Generate payment schedule
            if (TitlePawn.LoanAmountApproved > 0)
            {
                // InterestRate is already monthly percentage (e.g., 1.5 for 1.5% monthly)
                decimal monthlyInterestRate = TitlePawn.InterestRate / 100m;
                Schedule = _paymentScheduleService.GeneratePaymentSchedule(
                    TitlePawn.LoanAmountApproved,
                    monthlyInterestRate,
                    12);
            }

            return Page();
        }
    }
}
