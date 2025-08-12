using Microsoft.AspNetCore.Identity;

namespace NET9.API.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<IdentityUser?> ObtenerUsuario();
    }
}