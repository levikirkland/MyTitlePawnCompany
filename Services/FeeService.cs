using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Services
{
    public interface IFeeService
    {
        Task<bool> AddFeeAsync(int titlePawnId, string feeType, decimal amount, string? description, int companyId);
        Task<bool> WaiveFeeAsync(int feeId, string waiveReason, string waivedBy, int companyId);
        Task<bool> ApplyLateFeeAsync(int titlePawnId, int companyId);
        Task<IEnumerable<Fee>> GetTitlePawnFeesAsync(int titlePawnId);
        Task<decimal> GetTotalActiveFeesAsync(int titlePawnId);
    }

    public class FeeService : IFeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Add a fee to a title pawn (towing, late fee, repossession, etc.)
        /// </summary>
        public async Task<bool> AddFeeAsync(int titlePawnId, string feeType, decimal amount, string? description, int companyId)
        {
            var titlePawn = await _unitOfWork.TitlePawns.GetByIdAsync(titlePawnId);
            if (titlePawn == null || titlePawn.CompanyId != companyId)
                return false;

            var fee = new Fee
            {
                TitlePawnId = titlePawnId,
                FeeType = feeType,
                Amount = amount,
                Description = description,
                CompanyId = companyId,
                CreatedDate = DateTime.UtcNow,
                IsWaived = false
            };

            await _unitOfWork.Fees.AddAsync(fee);
            
            // Update title pawn's additional fees and remaining balance
            titlePawn.AdditionalFees += amount;
            titlePawn.RemainingBalance += amount;
            
            await _unitOfWork.TitlePawns.UpdateAsync(titlePawn);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Waive a fee (store manager or admin can remove/reduce fees)
        /// </summary>
        public async Task<bool> WaiveFeeAsync(int feeId, string waiveReason, string waivedBy, int companyId)
        {
            var fee = await _unitOfWork.Fees.GetByIdAsync(feeId);
            if (fee == null || fee.CompanyId != companyId)
                return false;

            if (fee.IsWaived)
                return false; // Already waived

            var titlePawn = await _unitOfWork.TitlePawns.GetByIdAsync(fee.TitlePawnId);
            if (titlePawn == null)
                return false;

            // Reduce the fees and balance
            titlePawn.AdditionalFees = Math.Max(0, titlePawn.AdditionalFees - fee.Amount);
            titlePawn.RemainingBalance = Math.Max(0, titlePawn.RemainingBalance - fee.Amount);

            // Mark fee as waived
            fee.IsWaived = true;
            fee.WaivedDate = DateTime.UtcNow;
            fee.WaivedBy = waivedBy;
            fee.WaiveReason = waiveReason;

            await _unitOfWork.Fees.UpdateAsync(fee);
            await _unitOfWork.TitlePawns.UpdateAsync(titlePawn);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Automatically apply late fee based on business days overdue
        /// Calculated from day after due date, only on business days, after accrual hour (default 6pm)
        /// </summary>
        public async Task<bool> ApplyLateFeeAsync(int titlePawnId, int companyId)
        {
            var titlePawn = await _unitOfWork.TitlePawns.GetTitlePawnWithPaymentsAsync(titlePawnId);
            if (titlePawn == null || titlePawn.CompanyId != companyId)
                return false;

            // Check if loan is overdue
            if (DateTime.UtcNow <= titlePawn.LoanMaturityDate)
                return false;

            // Get store for business days settings
            if (titlePawn.Store == null)
            {
                var store = await _unitOfWork.Stores.GetByIdAsync(titlePawn.StoreId);
                titlePawn.Store = store;
            }

            // Calculate accumulated late fees
            decimal accumulatedLateFees = titlePawn.CalculateLateFees(DateTime.UtcNow);
            
            if (accumulatedLateFees <= 0)
                return false;

            // Check how much late fee has already been applied
            var existingLateFees = await _unitOfWork.Fees.FindAsync(f => 
                f.TitlePawnId == titlePawnId && 
                f.FeeType == "Late Fee" && 
                !f.IsWaived);

            decimal alreadyAppliedLateFees = existingLateFees.Sum(f => f.Amount);

            // Calculate the difference - what still needs to be applied
            decimal remainingLateFees = accumulatedLateFees - alreadyAppliedLateFees;

            if (remainingLateFees <= 0)
                return false; // Already up to date

            // Calculate business days for description
            int businessDaysLate = titlePawn.CalculateBusinessDaysLate(DateTime.UtcNow);
            
            // Apply the remaining late fees
            return await AddFeeAsync(titlePawnId, "Late Fee", remainingLateFees, 
                $"Late fees for {businessDaysLate} business days overdue (${titlePawn.GetDailyLateRate():N2}/day)", 
                companyId);
        }

        /// <summary>
        /// Get all fees for a title pawn
        /// </summary>
        public async Task<IEnumerable<Fee>> GetTitlePawnFeesAsync(int titlePawnId)
        {
            var fees = await _unitOfWork.Fees.FindAsync(f => f.TitlePawnId == titlePawnId);
            return fees.OrderByDescending(f => f.CreatedDate);
        }

        /// <summary>
        /// Get total of non-waived fees for a title pawn
        /// </summary>
        public async Task<decimal> GetTotalActiveFeesAsync(int titlePawnId)
        {
            var fees = await GetTitlePawnFeesAsync(titlePawnId);
            return fees.Where(f => !f.IsWaived).Sum(f => f.Amount);
        }
    }
}
