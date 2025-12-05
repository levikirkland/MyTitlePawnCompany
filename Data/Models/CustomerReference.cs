using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class CustomerReference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ReferenceName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Relationship { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
