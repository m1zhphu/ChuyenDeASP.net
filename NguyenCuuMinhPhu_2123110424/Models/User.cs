using System.ComponentModel.DataAnnotations;

namespace SmartGarage.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "Advisor";

        public bool IsActive { get; set; } = true;

        // --- CÁC CỘT BỔ SUNG TỪ BA ---
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}