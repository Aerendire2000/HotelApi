using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelApi.Data;
using HotelApi.DTOs;
using HotelApi.Excepciones;
using HotelApi.Models;

namespace HotelApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelesController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public HotelesController(HotelDbContext context)
        {
            _context = context;
        }

        // GET api/hoteles?ciudad=Lima&pagina=1&tamano=10
        [HttpGet]
        public async Task<ActionResult<PaginadoDto<HotelLecturaDto>>> GetAll(
            [FromQuery] string? ciudad, [FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
        {
            pagina = Math.Max(pagina, 1);
            tamano = Math.Clamp(tamano, 1, 50);

            var consulta = _context.Hoteles.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(ciudad))
                consulta = consulta.Where(h => h.Ciudad == ciudad);

            var total = await consulta.CountAsync();

            var hoteles = await consulta
                .OrderBy(h => h.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(h => new HotelLecturaDto
                {
                    Id = h.Id,
                    Nombre = h.Nombre,
                    Ciudad = h.Ciudad,
                    Estrellas = h.Estrellas,
                    TotalHabitaciones = h.Habitaciones.Count,
                    PrecioDesde = h.Habitaciones.Where(x => x.Activa)
                                                .Min(x => (decimal?)x.PrecioPorNoche)
                })
                .ToListAsync();

            return new PaginadoDto<HotelLecturaDto>
            {
                Pagina = pagina, TamanoPagina = tamano, TotalRegistros = total, Datos = hoteles
            };
        }

        // GET api/hoteles/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HotelDetalleDto>> GetById(int id)
        {
            var hotel = await _context.Hoteles
                .AsNoTracking()
                .Where(h => h.Id == id)
                .Select(h => new HotelDetalleDto
                {
                    Id = h.Id,
                    Nombre = h.Nombre,
                    Direccion = h.Direccion,
                    Ciudad = h.Ciudad,
                    Estrellas = h.Estrellas,
                    Habitaciones = h.Habitaciones
                        .OrderBy(x => x.Numero)
                        .Select(x => new HabitacionResumenDto
                        {
                            Id = x.Id, Numero = x.Numero, Tipo = x.Tipo,
                            PrecioPorNoche = x.PrecioPorNoche, Activa = x.Activa
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return hotel ?? throw new NoEncontradoException($"No existe el hotel con Id {id}.");
        }

        // POST api/hoteles
        [HttpPost]
        public async Task<ActionResult<HotelDetalleDto>> Create([FromBody] HotelCrearDto dto)
        {
            var hotel = new Hotel
            {
                Nombre = dto.Nombre.Trim(),
                Direccion = dto.Direccion.Trim(),
                Ciudad = dto.Ciudad.Trim(),
                Estrellas = dto.Estrellas
            };

            _context.Hoteles.Add(hotel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = hotel.Id }, new HotelDetalleDto
            {
                Id = hotel.Id, Nombre = hotel.Nombre, Direccion = hotel.Direccion,
                Ciudad = hotel.Ciudad, Estrellas = hotel.Estrellas
            });
        }

        // PUT api/hoteles/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] HotelCrearDto dto)
        {
            var hotel = await _context.Hoteles.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el hotel con Id {id}.");

            hotel.Nombre = dto.Nombre.Trim();
            hotel.Direccion = dto.Direccion.Trim();
            hotel.Ciudad = dto.Ciudad.Trim();
            hotel.Estrellas = dto.Estrellas;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/hoteles/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] HotelPatchDto dto)
        {
            var hotel = await _context.Hoteles.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el hotel con Id {id}.");

            if (dto.Nombre is not null) hotel.Nombre = dto.Nombre.Trim();
            if (dto.Direccion is not null) hotel.Direccion = dto.Direccion.Trim();
            if (dto.Ciudad is not null) hotel.Ciudad = dto.Ciudad.Trim();
            if (dto.Estrellas is not null) hotel.Estrellas = dto.Estrellas.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/hoteles/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await _context.Hoteles.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el hotel con Id {id}.");

            if (await _context.Habitaciones.AnyAsync(h => h.HotelId == id))
                throw new ReglaNegocioException(
                    "No se puede eliminar: el hotel tiene habitaciones.");

            _context.Hoteles.Remove(hotel);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
