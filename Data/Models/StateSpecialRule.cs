using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class StateSpecialRule
    {
        [Key]
        public int Id { get; set; }

        public int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store? Store { get; set; }

        /// <summary>
        /// State code (e.g., "TX", "CA", "FL")
        /// </summary>
        [Required]
        [StringLength(2)]
        public string StateCode { get; set; } = string.Empty;

        /// <summary>
        /// Full state name
        /// </summary>
        [Required]
        [StringLength(50)]
        public string StateName { get; set; } = string.Empty;

        /// <summary>
        /// Period in days for the first tier (e.g., 90 days)
        /// </summary>
        public int FirstPeriodDays { get; set; } = 90;

        /// <summary>
        /// Maximum interest rate for first period (e.g., 25 for 25%)
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal FirstPeriodMaxRate { get; set; }

        /// <summary>
        /// Maximum interest rate after first period (e.g., 12.5 for 12.5%)
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal SubsequentPeriodMaxRate { get; set; }

        /// <summary>
        /// Additional rules or notes (markdown friendly)
        /// </summary>
        [StringLength(2000)]
        public string AdditionalRules { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Gets the maximum allowed rate based on days elapsed
        /// </summary>
        public decimal GetMaxRateForDaysElapsed(int daysElapsed)
        {
            if (daysElapsed <= FirstPeriodDays)
                return FirstPeriodMaxRate;
            
            return SubsequentPeriodMaxRate;
        }

        /// <summary>
        /// Validates if a given rate is within allowed limits
        /// </summary>
        public bool IsRateCompliant(decimal rate, int daysElapsed)
        {
            var maxRate = GetMaxRateForDaysElapsed(daysElapsed);
            return rate <= maxRate;
        }
    }
}
