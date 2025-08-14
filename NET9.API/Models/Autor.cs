using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NET9.API.Models
{
    public class Autor
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public required string Nombres { get; set; }
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public required string Apellido { get; set; }
        public string? Identificacion { get; set; }

        [Unicode(false)]
        public string? Foto { get; set; }
        public List<AutorLibro> Libros { get; set; } = [];
    }
}
