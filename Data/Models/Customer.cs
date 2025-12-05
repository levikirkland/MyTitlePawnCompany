using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTitlePawnCompany.Data.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? SSN { get; set; } // Encrypted in real app

        [StringLength(50)]
        public string? DriversLicense { get; set; }

        public DateTime DateOfBirth { get; set; }

        [StringLength(255)]
        public string? PlaceOfEmployment { get; set; }

        [StringLength(20)]
        public string? EmploymentPhoneNumber { get; set; }

        public int? YearsEmployed { get; set; }

        [StringLength(500)]
        public string? EmploymentAddress { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? MonthlyIncome { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<CustomerReference> References { get; set; } = new List<CustomerReference>();
    }
}
