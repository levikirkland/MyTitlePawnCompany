using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class InterestRateTier
    {
        [Key]
        public int Id { get; set; }

        public int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store? Store { get; set; }

        [Required]
        [StringLength(100)]
        public string TierName { get; set; } = string.Empty;

        /// <summary>
        /// Minimum principal amount for this tier (e.g., 0)
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal MinimumPrincipal { get; set; }

        /// <summary>
        /// Maximum principal amount for this tier (e.g., 500)
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal MaximumPrincipal { get; set; }

        /// <summary>
        /// Recommended interest rate for this tier (percentage per 30 days)
        /// Example: 1.5 for 1.5% per month
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Description/notes for this tier
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}
