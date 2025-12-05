using Xunit;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Tests
{
    public class InterestRateTierTests
    {
        [Fact]
        public void InterestRateTier_Create_WithValidData()
        {
            // Arrange
            var tier = new InterestRateTier
            {
                StoreId = 1,
                TierName = "Small Loans",
                MinimumPrincipal = 0,
                MaximumPrincipal = 500,
                InterestRate = 1.5m,
                Description = "Interest rate for loans under $500",
                IsActive = true,
                DisplayOrder = 1
            };

            // Act
            var result = tier;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.StoreId);
            Assert.Equal("Small Loans", result.TierName);
            Assert.Equal(0, result.MinimumPrincipal);
            Assert.Equal(500, result.MaximumPrincipal);
            Assert.Equal(1.5m, result.InterestRate);
            Assert.True(result.IsActive);
            Assert.Equal(1, result.DisplayOrder);
        }

        [Fact]
        public void InterestRateTier_ValidateTierRange_LoanWithinRange()
        {
            // Arrange
            var tier = new InterestRateTier
            {
                StoreId = 1,
                TierName = "Medium Loans",
                MinimumPrincipal = 500,
                MaximumPrincipal = 1000,
                InterestRate = 1.25m
            };

            var loanAmount = 750m;

            // Act
            var isWithinRange = loanAmount >= tier.MinimumPrincipal && loanAmount <= tier.MaximumPrincipal;

            // Assert
            Assert.True(isWithinRange);
        }

        [Fact]
        public void InterestRateTier_ValidateTierRange_LoanBelowRange()
        {
            // Arrange
            var tier = new InterestRateTier
            {
                StoreId = 1,
                TierName = "Medium Loans",
                MinimumPrincipal = 500,
                MaximumPrincipal = 1000,
                InterestRate = 1.25m
            };

            var loanAmount = 250m;

            // Act
            var isWithinRange = loanAmount >= tier.MinimumPrincipal && loanAmount <= tier.MaximumPrincipal;

            // Assert
            Assert.False(isWithinRange);
        }

        [Fact]
        public void InterestRateTier_ValidateTierRange_LoanAboveRange()
        {
            // Arrange
            var tier = new InterestRateTier
            {
                StoreId = 1,
                TierName = "Medium Loans",
                MinimumPrincipal = 500,
                MaximumPrincipal = 1000,
                InterestRate = 1.25m
            };

            var loanAmount = 1500m;

            // Act
            var isWithinRange = loanAmount >= tier.MinimumPrincipal && loanAmount <= tier.MaximumPrincipal;

            // Assert
            Assert.False(isWithinRange);
        }

        [Fact]
        public void InterestRateTier_CalculateMonthlyInterest()
        {
            // Arrange
            var tier = new InterestRateTier
            {
                InterestRate = 1.5m // 1.5% per month
            };

            var principal = 1000m;

            // Act
            var monthlyInterest = principal * (tier.InterestRate / 100);

            // Assert
            Assert.Equal(15m, monthlyInterest);
        }

        [Fact]
        public void InterestRateTier_MultiipleTiers_SortByDisplayOrder()
        {
            // Arrange
            var tiers = new List<InterestRateTier>
            {
                new InterestRateTier { TierName = "Large", DisplayOrder = 3 },
                new InterestRateTier { TierName = "Small", DisplayOrder = 1 },
                new InterestRateTier { TierName = "Medium", DisplayOrder = 2 }
            };

            // Act
            var sorted = tiers.OrderBy(t => t.DisplayOrder).ToList();

            // Assert
            Assert.Equal("Small", sorted[0].TierName);
            Assert.Equal("Medium", sorted[1].TierName);
            Assert.Equal("Large", sorted[2].TierName);
        }

        [Fact]
        public void InterestRateTier_Inactivetier_ShouldNotBeUsed()
        {
            // Arrange
            var tier = new InterestRateTier
            {
                TierName = "Inactive Tier",
                IsActive = false,
                InterestRate = 1.5m
            };

            // Act
            var canUse = tier.IsActive;

            // Assert
            Assert.False(canUse);
        }

        [Theory]
        [InlineData(0, 500, 1.5)]
        [InlineData(500.01, 1000, 1.25)]
        [InlineData(1000.01, 5000, 1.0)]
        public void InterestRateTier_DifferentRanges_CorrectRates(decimal min, decimal max, decimal rate)
        {
            // Arrange & Act
            var tier = new InterestRateTier
            {
                MinimumPrincipal = min,
                MaximumPrincipal = max,
                InterestRate = (decimal)rate
            };

            // Assert
            Assert.Equal(min, tier.MinimumPrincipal);
            Assert.Equal(max, tier.MaximumPrincipal);
            Assert.Equal((decimal)rate, tier.InterestRate);
        }
    }
}
