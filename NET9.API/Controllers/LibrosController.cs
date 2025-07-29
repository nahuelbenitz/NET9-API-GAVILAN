using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET9.API.Data;
using NET9.API.Models;

namespace NET9.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LibrosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Libro>> Get()
        {
            return await _context.Libros.Include(x => x.Autor).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Libro>> GetById(int id)
        {
            var libro = await _context.Libros.Include(x => x.Autor).FirstOrDefaultAsync(x => x.Id == id);

            if (libro is null)
            {
                return NotFound();
            }

            return libro;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Libro libro)
        {
            var existeAutor = await _context.Autores.AnyAsync(x => x.Id == libro.AutorId);
            if (!existeAutor)
            {
                ModelState.AddModelError(nameof(libro.AutorId), $"El autor con id {libro.AutorId} no existe");
                return ValidationProblem();
            }

            _context.Libros.Add(libro);

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = libro.Id }, libro);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] Libro libro)
        {
            if (id != libro.Id)
            {
                return BadRequest("Los id's son distintos");
            }

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