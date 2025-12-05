using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? StoreCode { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TitleAndKeyFee { get; set; } = 25.00m;

        // Business hours and late fee accrual settings
        public bool AccrueLateFeesSunday { get; set; } = false;
        public bool AccrueLateFeesSaturday { get; set; } = true;
        public int LateFeeAccrualHour { get; set; } = 18; // 6:00 PM (18:00 in 24-hour format)

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<TitlePawn> TitlePawns { get; set; } = new List<TitlePawn>();
        public virtual ICollection<StoreUser> StoreUsers { get; set; } = new List<StoreUser>();
    }
}
