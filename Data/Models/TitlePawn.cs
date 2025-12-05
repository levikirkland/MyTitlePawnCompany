using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class TitlePawn
    {
        [Key]
        public int Id { get; set; }

        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal LoanAmountRequested { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal LoanAmountApproved { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TitleAndKeyFee { get; set; } = 0m;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal AdditionalFees { get; set; } = 0m; // Towing, repossession, late fees, etc.

        [Column(TypeName = "decimal(5, 2)")]
        public decimal InterestRate { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal MonthlyPayment { get; set; }

        public int LoanTermDays { get; set; } = 30;

        public DateTime LoanStartDate { get; set; } = DateTime.UtcNow;

        public DateTime LoanMaturityDate { get; set; } = DateTime.UtcNow.AddDays(30);

        public DateTime? LoanPaidOffDate { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } = "Pending"; // Pending, Approved, Active, Paid Off, Defaulted

        [StringLength(500)]
        public string? ApprovalNotes { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalInterestCharged { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal RemainingBalance { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store? Store { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Contract fields
        public DateTime? ContractSignedDate { get; set; }
        
        public bool ContractSigned { get; set; } = false;

        // Renewal tracking
        public int? RenewedFromTitlePawnId { get; set; }

        [ForeignKey("RenewedFromTitlePawnId")]
        public virtual TitlePawn? RenewedFromTitlePawn { get; set; }

        // Navigation properties
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Fee> Fees { get; set; } = new List<Fee>();
        public virtual ICollection<TitlePawn> RenewedTitlePawns { get; set; } = new List<TitlePawn>();

        // Helper methods
        public decimal GetDailyLateRate()
        {
            // Daily late fee rate = Monthly Interest / 30 days
            return TotalInterestCharged / 30m;
        }

        public decimal CalculateLateFees(DateTime asOfDate)
        {
            // Calculate late fees from day after due date until asOfDate
            if (asOfDate <= LoanMaturityDate)
                return 0;

            // Find if any payments were made after due date to catch up
            var paymentsAfterDue = Payments?
                .Where(p => p.PaymentDate > LoanMaturityDate)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            // If caught up with a payment, no late fees apply
            if (paymentsAfterDue != null && paymentsAfterDue.Amount >= TotalInterestCharged)
                return 0;

            // Calculate business days late based on store settings
            int businessDaysLate = CalculateBusinessDaysLate(asOfDate);
            if (businessDaysLate < 1) return 0;

            return GetDailyLateRate() * businessDaysLate;
        }

        public int CalculateBusinessDaysLate(DateTime asOfDate)
        {
            if (asOfDate <= LoanMaturityDate || Store == null)
                return 0;

            int businessDays = 0;
            DateTime currentDate = LoanMaturityDate.Date.AddDays(1); // Start day after due date
            int accrualHour = Store.LateFeeAccrualHour;

            while (currentDate <= asOfDate.Date)
            {
                bool isBusinessDay = true;

                // Check if it's Sunday and Sunday is closed
                if (currentDate.DayOfWeek == DayOfWeek.Sunday && !Store.AccrueLateFeesSunday)
                    isBusinessDay = false;

                // Check if it's Saturday and Saturday is closed
                if (currentDate.DayOfWeek == DayOfWeek.Saturday && !Store.AccrueLateFeesSaturday)
                    isBusinessDay = false;

                // If it's today, only count if past the accrual hour
                if (currentDate.Date == asOfDate.Date)
                {
                    if (asOfDate.Hour < accrualHour)
                        isBusinessDay = false; // Haven't reached accrual time yet today
                }

                if (isBusinessDay)
                    businessDays++;

                currentDate = currentDate.AddDays(1);
            }

            return businessDays;
        }

        public bool IsStatus(string status)
        {
            return (Status ?? "").Equals(status, StringComparison.OrdinalIgnoreCase);
        }

        public decimal GetAmountToSatisfyLoan()
        {
            // Total amount needed to pay off loan = Principal + Interest + All Fees
            // Fees include: Title & Key, Towing, Repossession, Late Fees, etc.
            return LoanAmountApproved + TotalInterestCharged + TitleAndKeyFee + AdditionalFees;
        }

        public bool IsRenewed => Status == "Renewed";
        public bool IsActive => Status == "Active";
        public bool IsPaidOff => Status == "PaidOff" || Status == "Paid Off";
        public bool IsDefaulted => Status == "Defaulted";
        public bool IsPending => Status == "Pending";
    }
}
