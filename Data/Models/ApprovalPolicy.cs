using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class ApprovalPolicy
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Maximum loan amount user can approve (e.g., 500.00 = $500)
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ApprovalLimit { get; set; }

        /// <summary>
        /// Maximum number of loans per day
        /// </summary>
        public int DailyApprovalLimit { get; set; } = int.MaxValue;

        /// <summary>
        /// Maximum dollar amount per day
        /// </summary>
        [Column(TypeName = "decimal(12, 2)")]
        public decimal DailyApprovalAmount { get; set; } = decimal.MaxValue;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}
