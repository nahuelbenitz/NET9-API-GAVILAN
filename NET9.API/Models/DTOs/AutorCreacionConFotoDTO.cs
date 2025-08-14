namespace NET9.API.Models.DTOs
{
    public class AutorCreacionConFotoDTO :AutorCrearDTO
    {
        public IFormFile? Foto { get; set; }
    }
}
