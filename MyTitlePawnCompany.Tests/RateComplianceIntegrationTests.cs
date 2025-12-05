using Xunit;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Tests
{
    public class RateComplianceIntegrationTests
    {
        [Fact]
        public void ComplianceCheck_LoanWithinTierAndState()
        {
            // Arrange - Create a loan with tier and state rule
            var tier = new InterestRateTier
            {
                StoreId = 1,
                TierName = "Medium Loans",
                MinimumPrincipal = 500,
                MaximumPrincipal = 1000,
                InterestRate = 1.5m // Suggested rate
            };

            var stateRule = new StateSpecialRule
            {
                StateCode = "CA",
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m,
                SubsequentPeriodMaxRate = 12.5m
            };

            var principal = 750m;
            var proposedRate = 1.5m; // 1.5% per 30 days
            var loanStartDate = DateTime.UtcNow;

            // Act
            var isInTier = principal >= tier.MinimumPrincipal && principal <= tier.MaximumPrincipal;
            var daysElapsed = (DateTime.UtcNow - loanStartDate).Days;
            var isInFirstPeriod = daysElapsed <= stateRule.FirstPeriodDays;
            var maxStateRate = isInFirstPeriod ? stateRule.FirstPeriodMaxRate : stateRule.SubsequentPeriodMaxRate;
            var isStateCompliant = proposedRate <= maxStateRate;
            var isCompliant = isInTier && isStateCompliant;

            // Assert
            Assert.True(isInTier);
            Assert.True(isStateCompliant);
            Assert.True(isCompliant);
        }

        [Fact]
        public void ComplianceCheck_LoanExceedsStateRate()
        {
            // Arrange
            var stateRule = new StateSpecialRule
            {
                StateCode = "CA",
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m,
                SubsequentPeriodMaxRate = 12.5m
            };

            var proposedRate = 30m; // Exceeds 25% max
            var loanStartDate = DateTime.UtcNow;

            // Act
            var daysElapsed = (DateTime.UtcNow - loanStartDate).Days;
            var isInFirstPeriod = daysElapsed <= stateRule.FirstPeriodDays;
            var maxStateRate = isInFirstPeriod ? stateRule.FirstPeriodMaxRate : stateRule.SubsequentPeriodMaxRate;
            var isStateCompliant = proposedRate <= maxStateRate;

            // Assert
            Assert.False(isStateCompliant);
        }

        [Fact]
        public void ComplianceCheck_LoanAtTransitionPoint()
        {
            // Arrange - Loan at the boundary between first and subsequent period
            var stateRule = new StateSpecialRule
            {
                StateCode = "CA",
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m,
                SubsequentPeriodMaxRate = 12.5m
            };

            var loanStartDate = DateTime.UtcNow.AddDays(-89); // 89 days old
            var proposedRate = 25m;

            // Act - Still in first period
            var daysElapsed = (DateTime.UtcNow - loanStartDate).Days;
            var isInFirstPeriod = daysElapsed <= stateRule.FirstPeriodDays;
            var maxStateRate = isInFirstPeriod ? stateRule.FirstPeriodMaxRate : stateRule.SubsequentPeriodMaxRate;
            var isCompliant1 = proposedRate <= maxStateRate;

            // Now move past the boundary
            var loanStartDate2 = DateTime.UtcNow.AddDays(-91); // 91 days old
            var daysElapsed2 = (DateTime.UtcNow - loanStartDate2).Days;
            var isInFirstPeriod2 = daysElapsed2 <= stateRule.FirstPeriodDays;
            var maxStateRate2 = isInFirstPeriod2 ? stateRule.FirstPeriodMaxRate : stateRule.SubsequentPeriodMaxRate;
            var isCompliant2 = proposedRate <= maxStateRate2;

            // Assert
            Assert.True(isInFirstPeriod);
            Assert.True(isCompliant1);
            Assert.False(isInFirstPeriod2);
            Assert.False(isCompliant2); // 25% exceeds 12.5% subsequent period max
        }

        [Fact]
        public void ComplianceCheck_SelectAppropiateTier()
        {
            // Arrange
            var tiers = new List<InterestRateTier>
            {
                new InterestRateTier
                {
                    TierName = "Small",
                    MinimumPrincipal = 0,
                    MaximumPrincipal = 500,
                    InterestRate = 1.5m,
                    DisplayOrder = 1
                },
                new InterestRateTier
                {
                    TierName = "Medium",
                    MinimumPrincipal = 500.01m,
                    MaximumPrincipal = 1000,
                    InterestRate = 1.25m,
                    DisplayOrder = 2
                },
                new InterestRateTier
                {
                    TierName = "Large",
                    MinimumPrincipal = 1000.01m,
                    MaximumPrincipal = 5000,
                    InterestRate = 1.0m,
                    DisplayOrder = 3
                }
            };

            var principal = 750m;

            // Act
            var applicableTier = tiers.FirstOrDefault(t =>
                principal >= t.MinimumPrincipal &&
                principal <= t.MaximumPrincipal &&
                t.IsActive);

            // Assert
            Assert.NotNull(applicableTier);
            Assert.Equal("Medium", applicableTier.TierName);
            Assert.Equal(1.25m, applicableTier.InterestRate);
        }

        [Fact]
        public void ComplianceCheck_NoApplicableTier()
        {
            // Arrange
            var tiers = new List<InterestRateTier>
            {
                new InterestRateTier
                {
                    TierName = "Small",
                    MinimumPrincipal = 0,
                    MaximumPrincipal = 500,
                    InterestRate = 1.5m
                }
            };

            var principal = 1500m; // Above all tiers

            // Act
            var applicableTier = tiers.FirstOrDefault(t =>
                principal >= t.MinimumPrincipal &&
                principal <= t.MaximumPrincipal);

            // Assert
            Assert.Null(applicableTier);
        }

        [Fact]
        public void ComplianceCheck_CalculateTotalMonthlyPayment()
        {
            // Arrange
            var principal = 1000m;
            var rate = 1.5m; // 1.5% per month
            var tier = new InterestRateTier
            {
                InterestRate = rate
            };

            // Act
            var monthlyInterest = principal * (tier.InterestRate / 100);
            var suggestedMonthlyPayment = principal + monthlyInterest; // Principal + Interest

            // Assert
            Assert.Equal(15m, monthlyInterest);
            Assert.Equal(1015m, suggestedMonthlyPayment);
        }
    }
}
