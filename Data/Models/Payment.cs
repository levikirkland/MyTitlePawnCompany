using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int TitlePawnId { get; set; }

        [ForeignKey("TitlePawnId")]
        public virtual TitlePawn? TitlePawn { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        [StringLength(50)]
        public string? PaymentType { get; set; } = string.Empty; // Minimum, Extra, Payoff

        [StringLength(50)]
        public string? PaymentMethod { get; set; } = string.Empty; // Cash, Check, Card, Transfer

        [StringLength(500)]
        public string? Notes { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal LoanBalanceAfterPayment { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsLatePayment { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
