namespace NET9.API.Models.DTOs
{
    public class RespuestaAutenticacionDTO
    {
        public required string Token { get; set; }
        public DateTime Expiracion { get; set; }
    }
}
