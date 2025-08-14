using Microsoft.AspNetCore.Identity;
using NET9.API.Models;

namespace NET9.API.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario?> ObtenerUsuario();
    }
}