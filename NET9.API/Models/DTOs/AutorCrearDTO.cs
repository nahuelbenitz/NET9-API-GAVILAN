using System.ComponentModel.DataAnnotations;

namespace NET9.API.Models.DTOs
{
    public class AutorCrearDTO
    {
        [Required]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public required string Nombres { get; set; }
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public required string Apellido { get; set; }
        public string? Identificacion { get; set; }
        public List<LibroCreacionDTO> Libros { get; set; } = [];
    }
}
