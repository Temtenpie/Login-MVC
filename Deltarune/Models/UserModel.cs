using System.ComponentModel.DataAnnotations;

namespace Deltarune.Models  // 🔄 Cambia "tu_proyecto" por el nombre real de tu proyecto
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;
    }
}