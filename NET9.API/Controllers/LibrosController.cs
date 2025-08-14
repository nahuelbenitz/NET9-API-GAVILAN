using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET9.API.Data;
using NET9.API.Models;
using NET9.API.Models.DTOs;

namespace NET9.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "esadmin")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<LibroDTO>> Get()
        {
            var libros = await _context.Libros
                .Include(x => x.Autores)
                .ThenInclude(x => x.Autor)
                .ToListAsync();
            var librosDTO = _mapper.Map<IEnumerable<LibroDTO>>(libros);
            return librosDTO;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LibroConAutoresDTO>> GetById(int id)
        {
            var libro = await _context.Libros
                .Include(x => x.Autores)
                .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
            {
                return NotFound();
            }
            var libroDTO = _mapper.Map<LibroConAutoresDTO>(libro);

            return libroDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds is null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), "El libro debe tener al menos un autor");
                return ValidationProblem();
            }

            var autoresIdsExistentes = await _context.Autores
                .Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (autoresIdsExistentes.Count != libroCreacionDTO.AutoresIds.Count)
            {
                var autoresNoExisten = libroCreacionDTO.AutoresIds.Except(autoresIdsExistentes);
                var autoresNoExistenString = string.Join(", ", autoresNoExisten);
                var mensajeError = $"Los siguientes autores no existen: {autoresNoExistenString}";
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensajeError);
                return ValidationProblem();
            }

            var libro = _mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutores(libro);

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            var libroDTO = _mapper.Map<LibroDTO>(libro);
            return CreatedAtAction(nameof(GetById), new { id = libro.Id }, libroDTO);
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.Autores is not null)
            {
                for (int i = 0; i < libro.Autores.Count; i++)
                {
                    libro.Autores[i].Orden = i;
                }
            }

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds is null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), "El libro debe tener al menos un autor");
                return ValidationProblem();
            }

            var autoresIdsExistentes = await _context.Autores
                .Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (autoresIdsExistentes.Count != libroCreacionDTO.AutoresIds.Count)
            {
                var autoresNoExisten = libroCreacionDTO.AutoresIds.Except(autoresIdsExistentes);
                var autoresNoExistenString = string.Join(", ", autoresNoExisten);
                var mensajeError = $"Los siguientes autores no existen: {autoresNoExistenString}";
                ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensajeError);
                return ValidationProblem();
            }

            var libro = await _context.Libros.Include(x => x.Autores).FirstOrDefaultAsync(x => x.Id == id);
            if (libro is null)
            {
                return NotFound();
            }

            libro = _mapper.Map(libroCreacionDTO, libro);
            AsignarOrdenAutores(libro);

            _context.Update(libro);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var libroBorrado = await _context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (libroBorrado == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}