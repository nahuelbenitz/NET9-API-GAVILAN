using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NET9.API.Models
{
    public class Comentario
    {
        public Guid Id { get; set; }
        [Required]
        public required string Cuerpo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int LibroId { get; set; }
        public Libro? Libro { get; set; }
        public required string UsuarioID { get; set; }
        public IdentityUser? Usuario { get; set; }
    }
}
