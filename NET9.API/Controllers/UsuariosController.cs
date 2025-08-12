using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NET9.API.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NET9.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signinManager;

        public UsuariosController(UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signinManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signinManager = signinManager;
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuariosDTO credencialesDTO)
        {
            var usuario = new IdentityUser
            { 
                UserName = credencialesDTO.Email, 
                Email = credencialesDTO.Email 
            };

            var resultado = await _userManager.CreateAsync(usuario, credencialesDTO.Password!);

            if(resultado.Succeeded)
            {
                var respuestaAutenticacion = await ConstruirToken(credencialesDTO);

                return respuestaAutenticacion;
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return ValidationProblem();
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUsuariosDTO credencialesDTO)
        {
            var usuario = await _userManager.FindByEmailAsync(credencialesDTO.Email!);

            if (usuario == null)
            {
                return RetornarLoginIncorrecto();
            }

            var resultado = await _signinManager.CheckPasswordSignInAsync(usuario, credencialesDTO.Password!, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
            return await ConstruirToken(credencialesDTO);

            }
            else
            {
                return RetornarLoginIncorrecto();
            }
        }

        private ActionResult RetornarLoginIncorrecto()
        {
            ModelState.AddModelError(string.Empty, "Login incorrecto");
            return ValidationProblem();
        }

        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuariosDTO credencialesDTO)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesDTO.Email!)
            };

            var usuario = await _userManager.FindByEmailAsync(credencialesDTO.Email!);
            var claimsDB = await _userManager.GetClaimsAsync(usuario!);
            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddDays(1);
            
            var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);
            
            return new RespuestaAutenticacionDTO()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiracion = expiracion
            };
        }
    }
}
