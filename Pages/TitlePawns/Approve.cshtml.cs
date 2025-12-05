using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;
using MyTitlePawnCompany.Services;

namespace MyTitlePawnCompany.Pages.TitlePawns
{
    [Authorize(Roles = "CompanyAdmin")]
    public class ApproveModel : PageModel
    {
        private readonly ITitlePawnService _titlePawnService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRateRecommendationService _rateRecommendationService;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public TitlePawn? TitlePawn { get; set; }

        [BindProperty]
        public decimal ApprovedAmount { get; set; }

        [BindProperty]
        public decimal InterestRate { get; set; }

        [BindProperty]
        public bool ContractSigned { get; set; }

        public decimal MaxLoanAmount { get; set; }
        public decimal TitleAndKeyFee { get; set; }
        public int ReferenceCount { get; set; }
        public List<CustomerReference> References { get; set; } = new List<CustomerReference>();
        public string ErrorMessage { get; set; } = string.Empty;
        public decimal RecommendedRate { get; set; }
        public string? ApplicableTierName { get; set; }

        public ApproveModel(ITitlePawnService titlePawnService, IUnitOfWork unitOfWork, IRateRecommendationService rateRecommendationService, UserManager<ApplicationUser> userManager)
        {
            _titlePawnService = titlePawnService;
            _unitOfWork = unitOfWork;
            _rateRecommendationService = rateRecommendationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            TitlePawn = await _titlePawnService.GetTitlePawnAsync(id.Value);
            if (TitlePawn == null || TitlePawn.Status != "Pending")
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Get company information
            var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId);
            if (company == null)
                return NotFound();

            TitleAndKeyFee = company.TitleAndKeyFee;

            // Calculate max loan: 20% less than vehicle value, minus fees
            decimal vehicleValue = TitlePawn.Vehicle?.EstimatedValue ?? 0;
            MaxLoanAmount = (vehicleValue * 0.80m) - TitleAndKeyFee;

            ApprovedAmount = Math.Min(TitlePawn.LoanAmountRequested, MaxLoanAmount);
            
            // Get recommended rate based on loan amount and store tiers
            var loanAmount = TitlePawn.LoanAmountRequested;
            var recommendedRateResult = await _rateRecommendationService.GetRecommendedRateAsync(TitlePawn.StoreId, loanAmount);
            RecommendedRate = recommendedRateResult ?? 1.5m; // Default to 1.5% if no tier found
            InterestRate = RecommendedRate;

            // Get applicable tier info
            var applicableTier = await _rateRecommendationService.GetApplicableTierAsync(TitlePawn.StoreId, loanAmount);
            if (applicableTier != null)
            {
                ApplicableTierName = $"{applicableTier.TierName} (${applicableTier.MinimumPrincipal:N0} - ${applicableTier.MaximumPrincipal:N0})";
            }
            else
            {
                ApplicableTierName = "No tier found - using default 1.5%";
            }

            // Get customer references
            var vehicle = TitlePawn.Vehicle;
            if (vehicle != null)
            {
                References = (await _unitOfWork.CustomerReferences.GetCustomerReferencesAsync(vehicle.CustomerId)).ToList();
                ReferenceCount = References.Count;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            TitlePawn = await _titlePawnService.GetTitlePawnAsync(TitlePawn?.Id ?? 0);
            if (TitlePawn == null)
                return NotFound();

            // Validate references
            var vehicle = TitlePawn.Vehicle;
            if (vehicle != null)
            {
                References = (await _unitOfWork.CustomerReferences.GetCustomerReferencesAsync(vehicle.CustomerId)).ToList();
                ReferenceCount = References.Count;

                if (ReferenceCount < 3)
                {
                    ModelState.AddModelError("", $"Customer must have at least 3 references. Currently has {ReferenceCount}.");
                    return Page();
                }
            }

            // Validate contract signed
            if (!ContractSigned)
            {
                ModelState.AddModelError("ContractSigned", "You must confirm that the contract has been signed.");
                return Page();
            }

            if (!ModelState.IsValid)
                return Page();

            // Approve the loan
            var result = await _titlePawnService.ApproveTitlePawnAsync(TitlePawn.Id, ApprovedAmount, InterestRate, user.CompanyId);
            if (result == null)
                return NotFound();

            // Mark contract as signed
            result.ContractSigned = true;
            result.ContractSignedDate = DateTime.UtcNow;
            await _unitOfWork.TitlePawns.UpdateAsync(result);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Loan approved successfully!";
            return RedirectToPage("./Details", new { id = TitlePawn.Id });
        }
    }
}
