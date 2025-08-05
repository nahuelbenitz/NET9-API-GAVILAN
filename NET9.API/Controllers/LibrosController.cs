using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET9.API.Data;
using NET9.API.Models;
using NET9.API.Models.DTOs;

namespace NET9.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var libros = await _context.Libros.Include(x => x.Autor).ToListAsync();
            var librosDTO = _mapper.Map<IEnumerable<LibroDTO>>(libros);
            return librosDTO;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LibroConAutorDTO>> GetById(int id)
        {
            var libro = await _context.Libros.Include(x => x.Autor).FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
            {
                return NotFound();
            }
            var libroDTO = _mapper.Map<LibroConAutorDTO>(libro);
            
            return libroDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            var existeAutor = await _context.Autores.AnyAsync(x => x.Id == libroCreacionDTO.AutorId);
            if (!existeAutor)
            {
                ModelState.AddModelError(nameof(libroCreacionDTO.AutorId), $"El autor con id {libroCreacionDTO.AutorId} no existe");
                return ValidationProblem();
            }

            var libro = _mapper.Map<Libro>(libroCreacionDTO);
            _context.Libros.Add(libro);

            await _context.SaveChangesAsync();

            var libroDTO = _mapper.Map<LibroDTO>(libro);
            return CreatedAtAction(nameof(GetById), new { id = libro.Id }, libroDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            var libro = _mapper.Map<Libro>(libroCreacionDTO);
            libro.Id = id;

            var existeAutor = await _context.Autores.AnyAsync(x => x.Id == libro.AutorId);
            if (!existeAutor)
            {
                ModelState.AddModelError(nameof(libro.AutorId), $"El autor con id {libro.AutorId} no existe");
                return ValidationProblem();
            }

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