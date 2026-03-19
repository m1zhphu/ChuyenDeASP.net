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

        public string Role { get; set; } = "Staff"; // Admin, Advisor, Mechanic

        public bool IsActive { get; set; } = true;
    }
}