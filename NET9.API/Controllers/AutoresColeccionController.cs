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
    public class AutoresColeccionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AutoresColeccionController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{ids}")]
        public async Task<ActionResult<List<AutorConLibrosDTO>>> ObtenerAutoresPorIds(string ids)
        {
            var idsColeccion = new List<int>();

            foreach (var id in ids.Split(","))
            {
                if (int.TryParse(id, out int idInt))
                {
                    idsColeccion.Add(idInt);
                }
            }

            if (!idsColeccion.Any())
            {
                ModelState.AddModelError(nameof(ids), "No se han proporcionado IDs válidos.");
                return ValidationProblem();
            }

            var autores = await _context.Autores
                                    .Include(a => a.Libros)
                                    .ThenInclude(x => x.Libro)
                                    .Where(a => idsColeccion.Contains(a.Id))
                                    .ToListAsync();

            if (autores.Count != idsColeccion.Count)
            {
                return NotFound();
            }

            var autoresDTO = _mapper.Map<List<AutorConLibrosDTO>>(autores);
            return autoresDTO;
        }

        [HttpPost]
        public async Task<IActionResult> CrearAutoresColeccion([FromBody] IEnumerable<AutorCrearDTO> autoresCrearDTO)
        {
            var autores = _mapper.Map<IEnumerable<Autor>>(autoresCrearDTO);
            _context.Autores.AddRange(autores);
            await _context.SaveChangesAsync();

            var autoresDTO = _mapper.Map<List<AutorDTO>>(autores);
            var ids = autores.Select(a => a.Id);
            var idsString = string.Join(",", ids);

            return CreatedAtAction(nameof(ObtenerAutoresPorIds), new { ids = idsString }, autoresDTO);
        }

    }
}
