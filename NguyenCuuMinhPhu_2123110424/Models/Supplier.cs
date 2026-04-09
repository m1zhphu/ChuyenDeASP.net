using System.ComponentModel.DataAnnotations;

namespace SmartGarage.Models
{
    public class Supplier
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? TaxCode { get; set; } // Mã số thuế

        public bool IsActive { get; set; } = true;
    }
}