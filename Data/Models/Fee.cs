using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class Fee
    {
        [Key]
        public int Id { get; set; }

        public int TitlePawnId { get; set; }

        [ForeignKey("TitlePawnId")]
        public virtual TitlePawn? TitlePawn { get; set; }

        [Required]
        [StringLength(100)]
        public string FeeType { get; set; } = string.Empty; // "Late Fee", "Towing", "Repossession", "Key", "Title", etc.

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public bool IsWaived { get; set; } = false;

        public DateTime? WaivedDate { get; set; }

        [StringLength(255)]
        public string? WaivedBy { get; set; } // Store manager or admin who waived the fee

        [StringLength(500)]
        public string? WaiveReason { get; set; }
    }
}
