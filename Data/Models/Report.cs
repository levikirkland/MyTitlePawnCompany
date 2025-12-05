using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [StringLength(50)]
        public string ReportType { get; set; } = string.Empty; // Portfolio, Payment, Default, Revenue

        public DateTime ReportDate { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalLoansValue { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalReceivedPayments { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal OutstandingBalance { get; set; }

        public int ActiveLoans { get; set; }

        public int DefaultedLoans { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
