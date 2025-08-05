using System.ComponentModel.DataAnnotations;

namespace NET9.API.Models.DTOs
{
    public class ComentarioCrearDTO
    {
        [Required]
        public required string Cuerpo { get; set; }
    }
}
