using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Services
{
    public interface ITitlePawnService
    {
        Task<TitlePawn> CreateTitlePawnAsync(TitlePawn titlePawn);
        Task<TitlePawn?> GetTitlePawnAsync(int id);
        Task<TitlePawn?> ApproveTitlePawnAsync(int titlePawnId, decimal approvedAmount, decimal interestRate, int companyId);
        Task<bool> ProcessPaymentAsync(int titlePawnId, decimal amount, string paymentType, string paymentMethod, int companyId);
        Task UpdateTitlePawnAsync(TitlePawn titlePawn);
        Task<List<TitlePawn>> GetCompanyActiveLoansAsync(int companyId);
        Task<decimal> CalculateMinimumPaymentAsync(int titlePawnId);
        Task<bool> RenewLoanAsync(int titlePawnId);
        Task GenerateTitlePawnPrintAsync(int titlePawnId);
        Task<bool> AddAdditionalFeeAsync(int titlePawnId, decimal feeAmount, string feeDescription, int companyId);
    }

    public class TitlePawnService : ITitlePawnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentScheduleService _paymentScheduleService;

        public TitlePawnService(IUnitOfWork unitOfWork, IPaymentScheduleService paymentScheduleService)
        {
            _unitOfWork = unitOfWork;
            _paymentScheduleService = paymentScheduleService;
        }

        public async Task<TitlePawn> CreateTitlePawnAsync(TitlePawn titlePawn)
        {
            titlePawn.Status = "Pending";
            titlePawn.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.TitlePawns.AddAsync(titlePawn);
            await _unitOfWork.SaveChangesAsync();
            return titlePawn;
        }

        public async Task<TitlePawn?> GetTitlePawnAsync(int id)
        {
            return await _unitOfWork.TitlePawns.GetTitlePawnWithPaymentsAsync(id);
        }

        public async Task<TitlePawn?> ApproveTitlePawnAsync(int titlePawnId, decimal approvedAmount, decimal interestRate, int companyId)
        {
            var titlePawn = await _unitOfWork.TitlePawns.GetByIdAsync(titlePawnId);
            if (titlePawn == null || titlePawn.CompanyId != companyId)
                return null;

            // Get store to retrieve Title and Key fee
            var store = await _unitOfWork.Stores.GetByIdAsync(titlePawn.StoreId);
            var titleAndKeyFee = store?.TitleAndKeyFee ?? 0m;

            titlePawn.LoanAmountApproved = approvedAmount;
            titlePawn.TitleAndKeyFee = titleAndKeyFee;
            titlePawn.InterestRate = interestRate; // This is now monthly rate
            titlePawn.Status = "Active";
            // Remaining balance includes approved amount + fees (interest added later when paid)
            titlePawn.RemainingBalance = approvedAmount + titleAndKeyFee;
            
            // Calculate loan details
            titlePawn.LoanStartDate = DateTime.UtcNow;
            titlePawn.LoanMaturityDate = titlePawn.LoanStartDate.AddDays(titlePawn.LoanTermDays);
            
            // InterestRate is already monthly (e.g., 1.5 for 1.5% monthly)
            decimal monthlyRateDecimal = interestRate / 100m;
            decimal monthlyInterest = CalculateTotalInterest(approvedAmount, monthlyRateDecimal);
            
            // Minimum payment is interest only (loan renews for another 30 days with same principal)
            titlePawn.MonthlyPayment = monthlyInterest;
            
            // Total interest charged for one month
            titlePawn.TotalInterestCharged = monthlyInterest;

            await _unitOfWork.TitlePawns.UpdateAsync(titlePawn);
            await _unitOfWork.SaveChangesAsync();
            return titlePawn;
        }

        public async Task<bool> ProcessPaymentAsync(int titlePawnId, decimal amount, string paymentType, string paymentMethod, int companyId)
        {
            var titlePawn = await _unitOfWork.TitlePawns.GetTitlePawnWithPaymentsAsync(titlePawnId);
            if (titlePawn == null || titlePawn.CompanyId != companyId)
                return false;

            // Check if interest was already paid this month
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var paymentsThisMonth = titlePawn.Payments?
                .Where(p => p.PaymentDate.Month == currentMonth && p.PaymentDate.Year == currentYear)
                .ToList() ?? new List<Payment>();

            var interestAlreadyPaid = paymentsThisMonth.Any(p => p.Amount >= titlePawn.MonthlyPayment);
            
            // Minimum payment must be at least the interest amount
            if (amount < titlePawn.TotalInterestCharged)
                return false;

            // For payoff, validate that interest is included unless already paid
            if (paymentType == "Payoff" && !interestAlreadyPaid)
            {
                var minimumPayoffAmount = titlePawn.RemainingBalance + titlePawn.TotalInterestCharged;
                if (amount < minimumPayoffAmount)
                {
                    // Payoff amount doesn't include interest - invalid
                    return false;
                }
            }

            // Calculate how much goes to interest and principal
            decimal amountToInterest = Math.Min(amount, titlePawn.TotalInterestCharged);
            decimal amountToPrincipal = amount - amountToInterest;

            // Reduce remaining balance by ONLY the principal payment (not the full amount)
            titlePawn.RemainingBalance = Math.Max(0, titlePawn.RemainingBalance - amountToPrincipal);

            var payment = new Payment
            {
                TitlePawnId = titlePawnId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                PaymentType = paymentType,
                PaymentMethod = paymentMethod,
                DueDate = titlePawn.LoanMaturityDate,
                IsLatePayment = DateTime.UtcNow > titlePawn.LoanMaturityDate,
                CompanyId = companyId,
                // Balance after payment should reflect only principal reduction
                LoanBalanceAfterPayment = titlePawn.RemainingBalance
            };

            if (titlePawn.RemainingBalance <= 0)
            {
                // Loan is paid off
                titlePawn.Status = "PaidOff";
                titlePawn.LoanPaidOffDate = DateTime.UtcNow;
            }
            else if (amount >= titlePawn.TotalInterestCharged && paymentType != "Payoff")
            {
                // Minimum payment (interest) was made - create new account (renewal)
                // Calculate principal amount (balance minus fees)
                decimal principalAmount = titlePawn.RemainingBalance - titlePawn.TitleAndKeyFee - titlePawn.AdditionalFees;
                
                var newTitlePawn = new TitlePawn
                {
                    VehicleId = titlePawn.VehicleId,
                    LoanAmountRequested = titlePawn.LoanAmountRequested,
                    LoanAmountApproved = principalAmount, // Principal only
                    TitleAndKeyFee = titlePawn.TitleAndKeyFee, // Carry forward the title fee
                    AdditionalFees = titlePawn.AdditionalFees, // Carry forward additional fees
                    InterestRate = titlePawn.InterestRate,
                    Status = "Active",
                    RemainingBalance = titlePawn.RemainingBalance, // Keep current balance (includes all fees)
                    CompanyId = titlePawn.CompanyId,
                    StoreId = titlePawn.StoreId,
                    RenewedFromTitlePawnId = titlePawnId,
                    LoanStartDate = DateTime.UtcNow,
                    LoanTermDays = titlePawn.LoanTermDays,
                    CreatedDate = DateTime.UtcNow,
                    ContractSigned = false  // New contract needed
                };

                newTitlePawn.LoanMaturityDate = newTitlePawn.LoanStartDate.AddDays(newTitlePawn.LoanTermDays);

                // Calculate interest based on principal only (not balance which includes fees)
                decimal monthlyRateDecimal = titlePawn.InterestRate / 100m;
                newTitlePawn.TotalInterestCharged = CalculateTotalInterest(principalAmount, monthlyRateDecimal);
                newTitlePawn.MonthlyPayment = newTitlePawn.TotalInterestCharged;

                // Mark old account as renewed
                titlePawn.Status = "Renewed";

                // Save new account
                await _unitOfWork.TitlePawns.AddAsync(newTitlePawn);
            }

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.TitlePawns.UpdateAsync(titlePawn);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<TitlePawn>> GetCompanyActiveLoansAsync(int companyId)
        {
            var loans = await _unitOfWork.TitlePawns.GetActiveLoansAsync(companyId);
            return loans.ToList();
        }

        public async Task<decimal> CalculateMinimumPaymentAsync(int titlePawnId)
        {
            var titlePawn = await _unitOfWork.TitlePawns.GetByIdAsync(titlePawnId);
            if (titlePawn == null)
                return 0;

            // Minimum payment is interest only
            return titlePawn.MonthlyPayment;
        }

        public async Task<bool> RenewLoanAsync(int titlePawnId)
        {
            var titlePawn = await _unitOfWork.TitlePawns.GetByIdAsync(titlePawnId);
            if (titlePawn == null || titlePawn.Status != "Active")
                return false;

            titlePawn.LoanStartDate = DateTime.UtcNow;
            titlePawn.LoanMaturityDate = titlePawn.LoanStartDate.AddDays(titlePawn.LoanTermDays);

            await _unitOfWork.TitlePawns.UpdateAsync(titlePawn);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task GenerateTitlePawnPrintAsync(int titlePawnId)
        {
            var titlePawn = await _unitOfWork.TitlePawns.GetTitlePawnWithPaymentsAsync(titlePawnId);
            if (titlePawn == null)
                throw new InvalidOperationException("Title Pawn not found");

            // This would generate a print-ready document for signature
            // Implementation would depend on your document generation library (e.g., iText, SelectPdf)
            await Task.CompletedTask;
        }

        public async Task UpdateTitlePawnAsync(TitlePawn titlePawn)
        {
            await _unitOfWork.TitlePawns.UpdateAsync(titlePawn);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Add additional fees to a loan (towing, repossession, late fees, etc.)
        /// </summary>
        public async Task<bool> AddAdditionalFeeAsync(int titlePawnId, decimal feeAmount, string feeDescription, int companyId)
        {
            var titlePawn = await _unitOfWork.TitlePawns.GetByIdAsync(titlePawnId);
            if (titlePawn == null || titlePawn.CompanyId != companyId)
                return false;

            // Add fee to AdditionalFees (does not affect principal for interest calculation)
            titlePawn.AdditionalFees += feeAmount;
            
            // Update remaining balance to include the new fee
            titlePawn.RemainingBalance += feeAmount;

            await _unitOfWork.TitlePawns.UpdateAsync(titlePawn);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private decimal CalculateTotalInterest(decimal principal, decimal monthlyInterestRate)
        {
            return principal * monthlyInterestRate;
        }
    }
}
