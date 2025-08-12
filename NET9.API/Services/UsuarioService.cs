using Microsoft.AspNetCore.Identity;
using NET9.API.Services.Interfaces;

namespace NET9.API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public UsuarioService(UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public async Task<IdentityUser?> ObtenerUsuario()
        {
            var emailClaim = _contextAccessor.HttpContext!.User.Claims.Where(x => x.Type == "email").FirstOrDefault();

            if (emailClaim is null)
            {
                return null;
            }

            var email = emailClaim.Value;
            return await _userManager.FindByEmailAsync(email);
        }
    }
}
