using System.ComponentModel.DataAnnotations;

namespace Deltarune.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de Usuario")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]  // Oculta el texto ingresado
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}