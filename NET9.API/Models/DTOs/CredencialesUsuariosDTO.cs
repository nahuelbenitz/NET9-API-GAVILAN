using System.ComponentModel.DataAnnotations;

namespace NET9.API.Models.DTOs
{
    public class CredencialesUsuariosDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "El campo {0} debe ser un correo electrónico válido")]
        public required string Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
