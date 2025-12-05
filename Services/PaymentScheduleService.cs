using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Services
{
    /// <summary>
    /// Represents a single payment period in an amortization schedule
    /// </summary>
    public class PaymentScheduleItem
    {
        public int Period { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal MonthlyInterest { get; set; }
        public decimal MinimumPayment { get; set; }
        public decimal PrincipalPlus5Percent { get; set; }
        public decimal PrincipalPlus10Percent { get; set; }
        public decimal EndingBalanceMinimum { get; set; }
        public decimal EndingBalancePlus5 { get; set; }
        public decimal EndingBalancePlus10 { get; set; }
    }

    public interface IPaymentScheduleService
    {
        List<PaymentScheduleItem> GeneratePaymentSchedule(decimal principal, decimal monthlyInterestRate, int numberOfPeriods = 12);
        decimal CalculateMonthlyInterest(decimal balance, decimal monthlyInterestRate);
        decimal CalculateMinimumPayment(decimal principal, decimal monthlyInterestRate);
        int CalculatePayoffMonths(decimal principal, decimal monthlyInterestRate, decimal paymentAmount);
    }

    public class PaymentScheduleService : IPaymentScheduleService
    {
        /// <summary>
        /// Generates a 12-month payment schedule showing minimum, minimum+5%, and minimum+10% paydown scenarios
        /// InterestRate parameter is already MONTHLY rate (e.g., 0.015 for 1.5% monthly = 18% annually)
        /// </summary>
        public List<PaymentScheduleItem> GeneratePaymentSchedule(decimal principal, decimal monthlyInterestRate, int numberOfPeriods = 12)
        {
            var schedule = new List<PaymentScheduleItem>();
            
            // Calculate monthly interest on principal
            decimal monthlyInterest = principal * monthlyInterestRate;
            
            decimal balanceMinimum = principal;
            decimal balancePlus5 = principal;
            decimal balancePlus10 = principal;
            
            for (int i = 1; i <= numberOfPeriods; i++)
            {
                // Interest is calculated on current balance for each scenario
                decimal interestMinimum = balanceMinimum * monthlyInterestRate;
                decimal interestPlus5 = balancePlus5 * monthlyInterestRate;
                decimal interestPlus10 = balancePlus10 * monthlyInterestRate;
                
                // Minimum payment: interest only (loan renews for another 30 days)
                decimal paymentMinimum = interestMinimum;
                decimal endingBalanceMinimum = balanceMinimum; // Principal stays same, loan renews
                
                // Minimum + 5%: pay interest + 5% of original principal
                decimal principalPaymentPlus5 = principal * 0.05m;
                decimal paymentPlus5 = interestPlus5 + principalPaymentPlus5;
                decimal endingBalancePlus5 = Math.Max(0, balancePlus5 - principalPaymentPlus5);
                
                // Minimum + 10%: pay interest + 10% of original principal
                decimal principalPaymentPlus10 = principal * 0.10m;
                decimal paymentPlus10 = interestPlus10 + principalPaymentPlus10;
                decimal endingBalancePlus10 = Math.Max(0, balancePlus10 - principalPaymentPlus10);
                
                schedule.Add(new PaymentScheduleItem
                {
                    Period = i,
                    PaymentDate = DateTime.UtcNow.AddDays(30 * i),
                    StartingBalance = balanceMinimum,
                    MonthlyInterest = interestMinimum,
                    MinimumPayment = paymentMinimum,
                    PrincipalPlus5Percent = paymentPlus5,
                    PrincipalPlus10Percent = paymentPlus10,
                    EndingBalanceMinimum = endingBalanceMinimum,
                    EndingBalancePlus5 = endingBalancePlus5,
                    EndingBalancePlus10 = endingBalancePlus10
                });
                
                // Update balances for next iteration
                balancePlus5 = endingBalancePlus5;
                balancePlus10 = endingBalancePlus10;
            }
            
            return schedule;
        }

        /// <summary>
        /// Calculates the monthly interest based on balance and monthly interest rate
        /// monthlyInterestRate should be decimal form (e.g., 0.015 for 1.5%)
        /// </summary>
        public decimal CalculateMonthlyInterest(decimal balance, decimal monthlyInterestRate)
        {
            return balance * monthlyInterestRate;
        }

        /// <summary>
        /// Calculates minimum payment (interest only for renewal)
        /// monthlyInterestRate should be decimal form (e.g., 0.015 for 1.5%)
        /// </summary>
        public decimal CalculateMinimumPayment(decimal principal, decimal monthlyInterestRate)
        {
            return principal * monthlyInterestRate;
        }

        /// <summary>
        /// Calculates how many months to pay off the loan with a fixed payment amount
        /// monthlyInterestRate should be decimal form (e.g., 0.015 for 1.5%)
        /// </summary>
        public int CalculatePayoffMonths(decimal principal, decimal monthlyInterestRate, decimal paymentAmount)
        {
            if (paymentAmount <= principal * monthlyInterestRate)
                return 0; // Payment not enough to cover interest

            decimal balance = principal;
            int months = 0;

            while (balance > 0 && months < 360) // 30 years max
            {
                decimal interest = balance * monthlyInterestRate;
                decimal principalPayment = paymentAmount - interest;

                if (principalPayment <= 0)
                    return 0; // Can't pay down principal

                balance -= principalPayment;
                months++;
            }

            return months;
        }
    }
}
