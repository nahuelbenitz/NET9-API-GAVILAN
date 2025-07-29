using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET9.API.Data;
using NET9.API.Models;

namespace NET9.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AutoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Autor>> Get()
        {
            return await _context.Autores.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Autor>> GetById(int id)
        {
            var autor = await _context.Autores.Include(x => x.Libros).FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }

            return autor;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Autor autor)
        {
            if (autor is null)
            {
                return BadRequest();
            }
            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = autor.Id }, autor);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] Autor autor)
        {
            if (id != autor.Id)
            {
                return BadRequest("Los id's son distintos");
            }

            if(autor is null)
            {
                return BadRequest("El autor no puede estar vacio");
            }

            _context.Update(autor);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var autorBorrado = await _context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (autorBorrado == 0)
            {
                return NotFound();
            }

            //No usa el Save porque ejecuta el delete directamente en la base de datos
            return Ok();
        }
    }
}
