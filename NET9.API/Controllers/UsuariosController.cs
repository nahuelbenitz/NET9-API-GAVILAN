using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NET9.API.Models.DTOs;
using NET9.API.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NET9.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signinManager, IUsuarioService usuarioService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signinManager = signinManager;
            _usuarioService = usuarioService;
        }

        [HttpPost("registro")]
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

        [HttpGet("renovar-token")]
        [Authorize]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> RenovarToken()
        {
            var usuario = await _usuarioService.ObtenerUsuario();
            if (usuario is null)
            {
                return NotFound();
            }

            var credencialesDTO = new CredencialesUsuariosDTO
            {
                Email = usuario.Email!
            };


            return await ConstruirToken(credencialesDTO);
        }

        [HttpPost("hacer-admin")]
        [Authorize(Policy = "esadmin")]
        public async Task<ActionResult> HacerAdmin(EditarClaimDTO credencialesDTO)
        {
            var usuario = await _userManager.FindByEmailAsync(credencialesDTO.Email!);
            if(usuario is null)
            {
                return NotFound();
            }

            await _userManager.AddClaimAsync(usuario, new Claim("esadmin", "true"));
            return NoContent();
        }

        [HttpPost("remover-admin")]
        [Authorize(Policy = "esadmin")]
        public async Task<ActionResult> RemoverAdmin(EditarClaimDTO credencialesDTO)
        {
            var usuario = await _userManager.FindByEmailAsync(credencialesDTO.Email!);
            if (usuario is null)
            {
                return NotFound();
            }

            await _userManager.RemoveClaimAsync(usuario, new Claim("esadmin", "true"));
            return NoContent();
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
