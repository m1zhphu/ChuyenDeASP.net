using System.ComponentModel.DataAnnotations;

namespace SmartGarage.Models
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property: Một khách hàng có thể sở hữu nhiều xe
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}