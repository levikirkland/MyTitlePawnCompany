using MyTitlePawnCompany.Data;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Services
{
    public interface IRateRecommendationService
    {
        Task<decimal?> GetRecommendedRateAsync(int storeId, decimal loanAmount);
        Task<InterestRateTier?> GetApplicableTierAsync(int storeId, decimal loanAmount);
    }

    public class RateRecommendationService : IRateRecommendationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RateRecommendationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets the recommended interest rate based on loan amount and store tiers
        /// </summary>
        public async Task<decimal?> GetRecommendedRateAsync(int storeId, decimal loanAmount)
        {
            var tier = await GetApplicableTierAsync(storeId, loanAmount);
            return tier?.InterestRate;
        }

        /// <summary>
        /// Gets the applicable interest rate tier for a given loan amount
        /// </summary>
        public async Task<InterestRateTier?> GetApplicableTierAsync(int storeId, decimal loanAmount)
        {
            if (loanAmount <= 0)
                return null;

            // Get all active tiers for the store, ordered by display order
            var tiers = (await _unitOfWork.InterestRateTiers.GetStoreRateTiersAsync(storeId)).ToList();

            // Find the tier that matches the loan amount
            var applicableTier = tiers.FirstOrDefault(t =>
                loanAmount >= t.MinimumPrincipal &&
                loanAmount <= t.MaximumPrincipal);

            return applicableTier;
        }
    }
}
