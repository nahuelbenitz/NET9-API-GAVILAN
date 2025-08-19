using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using NET9.API.Data;
using NET9.API.Models;
using NET9.API.Models.DTOs;
using NET9.API.Services.Interfaces;
using NET9.API.Utilidades;
using System.Linq.Dynamic.Core;

namespace NET9.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "esadmin")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAlmacenadorArchivo _almacenadorArchivo;
        private readonly ILogger<AutoresController> _logger;
        private readonly IOutputCacheStore _outPutCacheStore;
        private const string contenedor = "autores";
        private const string cache = "autores-cache";

        public AutoresController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivo almacenadorArchivo, ILogger<AutoresController> logger, IOutputCacheStore outPutCacheStore)
        {
            _context = context;
            _mapper = mapper;
            _almacenadorArchivo = almacenadorArchivo;
            _logger = logger;
            _outPutCacheStore = outPutCacheStore;
        }

        [HttpGet]
        [AllowAnonymous]
        [EndpointSummary("Obtiene una lista de autores")]
        [OutputCache(Tags = [cache])]
        public async Task<IEnumerable<AutorDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryble = _context.Autores.AsQueryable();

            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryble);

            var autores = await queryble.OrderBy(x => x.Nombres).Paginar(paginacionDTO).ToListAsync();

            var autoresDTO = _mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<AutorConLibrosDTO>> GetById(int id)
        {
            var autor = await _context.Autores.Include(x => x.Libros).FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }

            var autorDTO = _mapper.Map<AutorConLibrosDTO>(autor);

            return autorDTO;
        }

        [HttpGet("filtrar")]
        [AllowAnonymous]
        public async Task<ActionResult> Filtrar([FromQuery] AutorFiltroDTO autorFiltroDTO)
        {
            var queryable = _context.Autores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(autorFiltroDTO.Nombres))
            {
                queryable = queryable.Where(x => x.Nombres.Contains(autorFiltroDTO.Nombres));
            }
            if (!string.IsNullOrWhiteSpace(autorFiltroDTO.Apellidos))
            {
                queryable = queryable.Where(x => x.Apellido.Contains(autorFiltroDTO.Apellidos));
            }

            if (autorFiltroDTO.IncluirLibros)
            {
                queryable = queryable.Include(x => x.Libros).ThenInclude(x => x.Libro);
            }

            if (autorFiltroDTO.TieneFoto.HasValue)
            {
                if (autorFiltroDTO.TieneFoto.Value)
                {
                    queryable = queryable.Where(x => x.Foto != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Foto == null);
                }
            }

            if (autorFiltroDTO.TieneLibros.HasValue)
            {
                if (autorFiltroDTO.TieneLibros.Value)
                {
                    queryable = queryable.Where(x => x.Libros.Any());
                }
                else
                {
                    queryable = queryable.Where(x => !x.Libros.Any());
                }
            }

            if (!string.IsNullOrWhiteSpace(autorFiltroDTO.TituloLibro))
            {
                queryable = queryable.Where(x => x.Libros.Any(libro => libro.Libro!.Titulo.Contains(autorFiltroDTO.TituloLibro)));
            }

            if (!string.IsNullOrWhiteSpace(autorFiltroDTO.CampoOrdenar))
            {
                var tipoOrden = autorFiltroDTO.OrdenAscendente ? "ascending" : "descending";
                try
                {
                    queryable = queryable.OrderBy($"{autorFiltroDTO.CampoOrdenar} {tipoOrden}");

                }
                catch (Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Nombres);
                    _logger.LogError(ex.Message,ex);
                }

            }
            else
            {
                queryable = queryable.OrderBy(x => x.Nombres);
            }

            var autores = await queryable
                .Paginar(autorFiltroDTO.PaginacionDTO)
                .ToListAsync();

            if (autorFiltroDTO.IncluirLibros)
            {
                var autoresDTO = _mapper.Map<IEnumerable<AutorConLibrosDTO>>(autores);
                return Ok(autoresDTO);
            }
            else
            {
                var autoresDTO = _mapper.Map<IEnumerable<AutorDTO>>(autores);
                return Ok(autoresDTO);
            }


        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AutorCrearDTO autorCrearDTO)
        {
            if (autorCrearDTO is null)
            {
                return BadRequest();
            }
            var autor = _mapper.Map<Autor>(autorCrearDTO);
            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            await _outPutCacheStore.EvictByTagAsync(cache, default);
            
            var autorDTO = _mapper.Map<AutorDTO>(autor);
            return CreatedAtAction(nameof(GetById), new { id = autor.Id }, autorDTO);
        }


        [HttpPost("con-foto")]
        public async Task<ActionResult> PostConFoto([FromForm] AutorCreacionConFotoDTO autorCrearDTO)
        {
            if (autorCrearDTO is null)
            {
                return BadRequest();
            }

            var autor = _mapper.Map<Autor>(autorCrearDTO);

            if (autorCrearDTO.Foto is not null)
            {
                var url = await _almacenadorArchivo.Guardar(contenedor, autorCrearDTO.Foto);
                autor.Foto = url;
            }

            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();
            await _outPutCacheStore.EvictByTagAsync(cache, default);
            var autorDTO = _mapper.Map<AutorDTO>(autor);
            return CreatedAtAction(nameof(GetById), new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] AutorCreacionConFotoDTO autorCrearDTO)
        {

            var existeAutor = await _context.Autores.AnyAsync(x => x.Id == id);

            if (!existeAutor)
            {
                return NotFound();
            }

            if (autorCrearDTO is null)
            {
                return BadRequest("El autor no puede estar vacio");
            }

            var autor = _mapper.Map<Autor>(autorCrearDTO);
            autor.Id = id;

            if (autorCrearDTO.Foto is not null)
            {
                var fotoActual = await _context.Autores.Where(x => x.Id == id).Select(x => x.Foto).FirstAsync();

                var url = await _almacenadorArchivo.Editar(contenedor, autorCrearDTO.Foto, fotoActual!);
                autor.Foto = url;
            }

            _context.Update(autor);

            await _context.SaveChangesAsync();

            await _outPutCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<AutorPatchDTO> patchDocument)
        {
            if (patchDocument is null)
            {
                return BadRequest("El documento de parcheo no puede estar vacio");
            }
            var autor = await _context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            if (autor is null)
            {
                return NotFound();
            }

            var autorPatchDTO = _mapper.Map<AutorPatchDTO>(autor);

            patchDocument.ApplyTo(autorPatchDTO, ModelState);

            //Otra forma de validar el modelo
            //if (!ModelState.IsValid)
            //{
            //    return ValidationProblem(ModelState);
            //}

            var esValido = TryValidateModel(autorPatchDTO);
            if (!esValido)
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(autorPatchDTO, autor);

            await _context.SaveChangesAsync();
            await _outPutCacheStore.EvictByTagAsync(cache, default);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var autor = await _context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            if (autor is null)
            {
                return NotFound();
            }
            _context.Remove(autor);
            await _context.SaveChangesAsync();
            await _outPutCacheStore.EvictByTagAsync(cache, default);
            await _almacenadorArchivo.Borrar(autor.Foto, contenedor);

            //var autorBorrado = await _context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();
            //if (autorBorrado == 0)
            //{
            //    return NotFound();
            //}

            //No usa el Save porque ejecuta el delete directamente en la base de datos
            return NoContent();
        }
    }
}
