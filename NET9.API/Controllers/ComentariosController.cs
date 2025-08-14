using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET9.API.Data;
using NET9.API.Models;
using NET9.API.Models.DTOs;
using NET9.API.Services.Interfaces;

namespace NET9.API.Controllers
{
    [Route("api/libros/{libroId:int}/[controller]")]
    [ApiController]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUsuarioService _usuarioService;

        public ComentariosController(ApplicationDbContext context, IMapper mapper, IUsuarioService usuarioService)
        {
            _context = context;
            _mapper = mapper;
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await _context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
            {
                return NotFound($"El libro con id {libroId} no existe");
            }

            var comentarios = await _context.Comentarios
                .Include(x => x.Usuario)
                .Where(x => x.LibroId == libroId)
                .OrderByDescending(x => x.FechaPublicacion)
                .ToListAsync();

            return _mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ComentarioDTO>> GetById(Guid id)
        {
            var comentario = await _context.Comentarios
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (comentario is null)
            {
                return NotFound();
            }
            return _mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPost]
        public async Task<ActionResult> Post(int libroId, [FromBody] ComentarioCrearDTO comentarioCrearDTO)
        {
            var libroExiste = await _context.Libros.AnyAsync(x => x.Id == libroId);

            if (!libroExiste)
            {
                return NotFound($"El libro con id {libroId} no existe");
            }

            var usuario = await _usuarioService.ObtenerUsuario();

            if(usuario is null)
            {
                return NotFound();
            }

            var comentario = _mapper.Map<Comentario>(comentarioCrearDTO);
            comentario.LibroId = libroId;
            comentario.FechaPublicacion = DateTime.UtcNow;
            comentario.UsuarioID = usuario.Id;

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            var comentarioDTO = _mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtAction(nameof(GetById), new { libroId, id = comentario.Id }, comentarioDTO);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, int libroId, [FromBody] JsonPatchDocument<ComentarioPatchDTO> patchDocument)
        {

            if (patchDocument is null)
            {
                return BadRequest("El documento de parcheo no puede estar vacio");
            }

            var libroExiste = await _context.Libros.AnyAsync(x => x.Id == libroId);

            if (!libroExiste)
            {
                return NotFound($"El libro con id {libroId} no existe");
            }

            var usuario = await _usuarioService.ObtenerUsuario();

            if (usuario is null)
            {
                return NotFound();
            }


            var comentario = await _context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentario is null)
            {
                return NotFound();
            }

            if (comentario.UsuarioID != usuario.Id)
            {
                return Forbid();
            }

            var comentarioPatchDTO = _mapper.Map<ComentarioPatchDTO>(comentario);

            patchDocument.ApplyTo(comentarioPatchDTO, ModelState);

            //Otra forma de validar el modelo
            //if (!ModelState.IsValid)
            //{
            //    return ValidationProblem(ModelState);
            //}

            var esValido = TryValidateModel(comentarioPatchDTO);
            if (!esValido)
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(comentarioPatchDTO, comentario);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, int libroId)
        {
            var usuario = await _usuarioService.ObtenerUsuario();

            if (usuario is null)
            {
                return NotFound();
            }

            var comentario = await _context.Comentarios
                .FirstOrDefaultAsync(x => x.Id == id);

            if (comentario is null)
            {
                return NotFound();
            }

            if (comentario.UsuarioID != usuario.Id)
            {
                return Forbid();
            }

            _context.Remove(comentario);
            await _context.SaveChangesAsync();
            //No usa el Save porque ejecuta el delete directamente en la base de datos
            return NoContent();
        }
    }
}
