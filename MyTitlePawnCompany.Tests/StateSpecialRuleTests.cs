using Xunit;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Tests
{
    public class StateSpecialRuleTests
    {
        [Fact]
        public void StateSpecialRule_Create_WithValidData()
        {
            // Arrange
            var rule = new StateSpecialRule
            {
                StoreId = 1,
                StateCode = "CA",
                StateName = "California",
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m,
                SubsequentPeriodMaxRate = 12.5m,
                AdditionalRules = "Requires written notice",
                IsActive = true
            };

            // Act
            var result = rule;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.StoreId);
            Assert.Equal("CA", result.StateCode);
            Assert.Equal("California", result.StateName);
            Assert.Equal(90, result.FirstPeriodDays);
            Assert.Equal(25m, result.FirstPeriodMaxRate);
            Assert.Equal(12.5m, result.SubsequentPeriodMaxRate);
            Assert.True(result.IsActive);
        }

        [Fact]
        public void StateSpecialRule_RateCompliance_WithinFirstPeriod()
        {
            // Arrange
            var rule = new StateSpecialRule
            {
                StateCode = "CA",
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m,
                SubsequentPeriodMaxRate = 12.5m
            };

            var loanStartDate = DateTime.UtcNow.AddDays(-30); // 30 days old (within 90 day period)
            var proposedRate = 20m;

            // Act
            var daysElapsed = (DateTime.UtcNow - loanStartDate).Days;
            var isInFirstPeriod = daysElapsed <= rule.FirstPeriodDays;
            var maxRate = isInFirstPeriod ? rule.FirstPeriodMaxRate : rule.SubsequentPeriodMaxRate;
            var isCompliant = proposedRate <= maxRate;

            // Assert
            Assert.True(isInFirstPeriod);
            Assert.Equal(25m, maxRate);
            Assert.True(isCompliant);
        }

        [Fact]
        public void StateSpecialRule_RateCompliance_AfterFirstPeriod()
        {
            // Arrange
            var rule = new StateSpecialRule
            {
                StateCode = "CA",
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m,
                SubsequentPeriodMaxRate = 12.5m
            };

            var loanStartDate = DateTime.UtcNow.AddDays(-120); // 120 days old (after 90 day period)
            var proposedRate = 12m;

            // Act
            var daysElapsed = (DateTime.UtcNow - loanStartDate).Days;
            var isInFirstPeriod = daysElapsed <= rule.FirstPeriodDays;
            var maxRate = isInFirstPeriod ? rule.FirstPeriodMaxRate : rule.SubsequentPeriodMaxRate;
            var isCompliant = proposedRate <= maxRate;

            // Assert
            Assert.False(isInFirstPeriod);
            Assert.Equal(12.5m, maxRate);
            Assert.True(isCompliant);
        }

        [Fact]
        public void StateSpecialRule_RateNonCompliance_ExceedsFirstPeriodMax()
        {
            // Arrange
            var rule = new StateSpecialRule
            {
                StateCode = "CA",
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m,
                SubsequentPeriodMaxRate = 12.5m
            };

            var loanStartDate = DateTime.UtcNow.AddDays(-30);
            var proposedRate = 30m; // Exceeds 25% max

            // Act
            var daysElapsed = (DateTime.UtcNow - loanStartDate).Days;
            var isInFirstPeriod = daysElapsed <= rule.FirstPeriodDays;
            var maxRate = isInFirstPeriod ? rule.FirstPeriodMaxRate : rule.SubsequentPeriodMaxRate;
            var isCompliant = proposedRate <= maxRate;

            // Assert
            Assert.True(isInFirstPeriod);
            Assert.False(isCompliant);
        }

        [Fact]
        public void StateSpecialRule_RateNonCompliance_ExceedsSubsequentMax()
        {
            // Arrange
            var rule = new StateSpecialRule
            {
                StateCode = "CA",
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m,
                SubsequentPeriodMaxRate = 12.5m
            };

            var loanStartDate = DateTime.UtcNow.AddDays(-120);
            var proposedRate = 15m; // Exceeds 12.5% max

            // Act
            var daysElapsed = (DateTime.UtcNow - loanStartDate).Days;
            var isInFirstPeriod = daysElapsed <= rule.FirstPeriodDays;
            var maxRate = isInFirstPeriod ? rule.FirstPeriodMaxRate : rule.SubsequentPeriodMaxRate;
            var isCompliant = proposedRate <= maxRate;

            // Assert
            Assert.False(isInFirstPeriod);
            Assert.False(isCompliant);
        }

        [Fact]
        public void StateSpecialRule_CalculateAnnualEquivalentRate_FirstPeriod()
        {
            // Arrange
            var monthlyRate = 25m; // 25% per 30 days

            // Act
            var annualEquivalent = monthlyRate * 12;

            // Assert
            Assert.Equal(300m, annualEquivalent); // 300% APR
        }

        [Fact]
        public void StateSpecialRule_CalculateAnnualEquivalentRate_SubsequentPeriod()
        {
            // Arrange
            var monthlyRate = 12.5m; // 12.5% per 30 days

            // Act
            var annualEquivalent = monthlyRate * 12;

            // Assert
            Assert.Equal(150m, annualEquivalent); // 150% APR
        }

        [Theory]
        [InlineData("CA", "California")]
        [InlineData("TX", "Texas")]
        [InlineData("NY", "New York")]
        [InlineData("FL", "Florida")]
        public void StateSpecialRule_ValidateStateCode(string code, string name)
        {
            // Arrange & Act
            var rule = new StateSpecialRule
            {
                StateCode = code,
                StateName = name
            };

            // Assert
            Assert.Equal(code, rule.StateCode);
            Assert.Equal(name, rule.StateName);
            Assert.Equal(2, rule.StateCode.Length);
        }

        [Fact]
        public void StateSpecialRule_MultipleStates_DifferentRates()
        {
            // Arrange
            var rules = new List<StateSpecialRule>
            {
                new StateSpecialRule
                {
                    StateCode = "CA",
                    StateName = "California",
                    FirstPeriodMaxRate = 25m,
                    SubsequentPeriodMaxRate = 12.5m
                },
                new StateSpecialRule
                {
                    StateCode = "TX",
                    StateName = "Texas",
                    FirstPeriodMaxRate = 20m,
                    SubsequentPeriodMaxRate = 20m
                }
            };

            // Act
            var caRule = rules.First(r => r.StateCode == "CA");
            var txRule = rules.First(r => r.StateCode == "TX");

            // Assert
            Assert.Equal(25m, caRule.FirstPeriodMaxRate);
            Assert.Equal(20m, txRule.FirstPeriodMaxRate);
            Assert.Equal(12.5m, caRule.SubsequentPeriodMaxRate);
            Assert.Equal(20m, txRule.SubsequentPeriodMaxRate);
        }

        [Fact]
        public void StateSpecialRule_InactiveRule_ShouldNotBeEnforced()
        {
            // Arrange
            var rule = new StateSpecialRule
            {
                StateCode = "CA",
                IsActive = false,
                FirstPeriodMaxRate = 25m
            };

            // Act
            var canEnforce = rule.IsActive;

            // Assert
            Assert.False(canEnforce);
        }

        [Fact]
        public void StateSpecialRule_EdgeCase_AtBoundary()
        {
            // Arrange
            var rule = new StateSpecialRule
            {
                FirstPeriodDays = 90,
                FirstPeriodMaxRate = 25m
            };

            var loanStartDate = DateTime.UtcNow.AddDays(-90); // Exactly at boundary
            var proposedRate = 25m; // Exactly at max

            // Act
            var daysElapsed = (DateTime.UtcNow - loanStartDate).Days;
            var isInFirstPeriod = daysElapsed <= rule.FirstPeriodDays;
            var isCompliant = proposedRate <= rule.FirstPeriodMaxRate;

            // Assert
            Assert.True(isInFirstPeriod);
            Assert.True(isCompliant);
        }

        [Fact]
        public void StateSpecialRule_ValidateCreatedDate()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;
            var rule = new StateSpecialRule
            {
                StateCode = "CA",
                CreatedDate = DateTime.UtcNow
            };
            var afterCreation = DateTime.UtcNow;

            // Act & Assert
            Assert.True(rule.CreatedDate >= beforeCreation);
            Assert.True(rule.CreatedDate <= afterCreation);
        }
    }
}
