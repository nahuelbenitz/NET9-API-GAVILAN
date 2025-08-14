using Microsoft.AspNetCore.Identity;

namespace NET9.API.Models
{
    public class Usuario : IdentityUser
    {
        public DateTime FechaNacimiento { get; set; }
    }
}
