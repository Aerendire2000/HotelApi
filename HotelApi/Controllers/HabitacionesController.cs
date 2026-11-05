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
    public class HabitacionesController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public HabitacionesController(HotelDbContext context)
        {
            _context = context;
        }

        // GET api/habitaciones?hotelId=1&tipo=Doble&soloActivas=true
        [HttpGet]
        public async Task<ActionResult<List<HabitacionLecturaDto>>> GetAll(
            [FromQuery] int? hotelId, [FromQuery] string? tipo,
            [FromQuery] bool soloActivas = false)
        {
            var consulta = _context.Habitaciones.AsNoTracking();
            if (hotelId is not null) consulta = consulta.Where(h => h.HotelId == hotelId);
            if (!string.IsNullOrWhiteSpace(tipo)) consulta = consulta.Where(h => h.Tipo == tipo);
            if (soloActivas) consulta = consulta.Where(h => h.Activa);

            return await consulta
                .OrderBy(h => h.HotelId).ThenBy(h => h.Numero)
                .Select(h => new HabitacionLecturaDto
                {
                    Id = h.Id,
                    Hotel = h.Hotel!.Nombre,
                    Numero = h.Numero,
                    Tipo = h.Tipo,
                    PrecioPorNoche = h.PrecioPorNoche,
                    Capacidad = h.Capacidad,
                    Activa = h.Activa
                })
                .ToListAsync();
        }

        // GET api/habitaciones/disponibles?entrada=2026-12-20&salida=2026-12-23&hotelId=1
        [HttpGet("disponibles")]
        public async Task<ActionResult<List<HabitacionLecturaDto>>> GetDisponibles(
            [FromQuery] DateOnly entrada, [FromQuery] DateOnly salida, [FromQuery] int? hotelId)
        {
            if (salida <= entrada)
                throw new SolicitudInvalidaException(
                    "La fecha de salida debe ser posterior a la de entrada.");

            var consulta = _context.Habitaciones.AsNoTracking().Where(h => h.Activa);
            if (hotelId is not null) consulta = consulta.Where(h => h.HotelId == hotelId);

            consulta = consulta.Where(h => !_context.ReservaDetalles.Any(d =>
                d.HabitacionId == h.Id &&
                d.Reserva!.Estado != "Cancelada" &&
                d.Reserva.FechaEntrada < salida &&
                entrada < d.Reserva.FechaSalida));

            return await consulta
                .OrderBy(h => h.PrecioPorNoche)
                .Select(h => new HabitacionLecturaDto
                {
                    Id = h.Id, Hotel = h.Hotel!.Nombre, Numero = h.Numero, Tipo = h.Tipo,
                    PrecioPorNoche = h.PrecioPorNoche, Capacidad = h.Capacidad, Activa = h.Activa
                })
                .ToListAsync();
        }

        // GET api/habitaciones/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HabitacionLecturaDto>> GetById(int id)
        {
            var habitacion = await _context.Habitaciones
                .AsNoTracking()
                .Where(h => h.Id == id)
                .Select(h => new HabitacionLecturaDto
                {
                    Id = h.Id, Hotel = h.Hotel!.Nombre, Numero = h.Numero, Tipo = h.Tipo,
                    PrecioPorNoche = h.PrecioPorNoche, Capacidad = h.Capacidad, Activa = h.Activa
                })
                .FirstOrDefaultAsync();

            return habitacion
                ?? throw new NoEncontradoException($"No existe la habitación con Id {id}.");
        }

        // POST api/habitaciones
        [HttpPost]
        public async Task<ActionResult<HabitacionLecturaDto>> Create(
            [FromBody] HabitacionCrearDto dto)
        {
            var hotel = await _context.Hoteles
                .AsNoTracking()
                .Where(h => h.Id == dto.HotelId)
                .Select(h => h.Nombre)
                .FirstOrDefaultAsync()
                ?? throw new NoEncontradoException($"No existe el hotel con Id {dto.HotelId}.");

            if (await _context.Habitaciones.AnyAsync(h =>
                    h.HotelId == dto.HotelId && h.Numero == dto.Numero))
                throw new ReglaNegocioException($"El hotel ya tiene una habitación {dto.Numero}.");

            var habitacion = new Habitacion
            {
                HotelId = dto.HotelId,
                Numero = dto.Numero,
                Tipo = dto.Tipo,
                PrecioPorNoche = dto.PrecioPorNoche,
                Capacidad = dto.Capacidad,
                Activa = dto.Activa
            };

            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync();

            var creada = new HabitacionLecturaDto
            {
                Id = habitacion.Id, Hotel = hotel, Numero = habitacion.Numero,
                Tipo = habitacion.Tipo,
                PrecioPorNoche = habitacion.PrecioPorNoche, Capacidad = habitacion.Capacidad,
                Activa = habitacion.Activa
            };

            return CreatedAtAction(nameof(GetById), new { id = habitacion.Id }, creada);
        }

        // PUT api/habitaciones/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] HabitacionCrearDto dto)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe la habitación con Id {id}.");

            if (!await _context.Hoteles.AnyAsync(h => h.Id == dto.HotelId))
                throw new NoEncontradoException($"No existe el hotel con Id {dto.HotelId}.");

            if (await _context.Habitaciones.AnyAsync(h =>
                    h.HotelId == dto.HotelId && h.Numero == dto.Numero && h.Id != id))
                throw new ReglaNegocioException(
                    $"El hotel ya tiene otra habitación {dto.Numero}.");

            habitacion.HotelId = dto.HotelId;
            habitacion.Numero = dto.Numero;
            habitacion.Tipo = dto.Tipo;
            habitacion.PrecioPorNoche = dto.PrecioPorNoche;
            habitacion.Capacidad = dto.Capacidad;
            habitacion.Activa = dto.Activa;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/habitaciones/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] HabitacionPatchDto dto)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe la habitación con Id {id}.");

            if (dto.Tipo is not null) habitacion.Tipo = dto.Tipo;
            if (dto.PrecioPorNoche is not null)
                habitacion.PrecioPorNoche = dto.PrecioPorNoche.Value;
            if (dto.Capacidad is not null) habitacion.Capacidad = dto.Capacidad.Value;
            if (dto.Activa is not null) habitacion.Activa = dto.Activa.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/habitaciones/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe la habitación con Id {id}.");

            if (await _context.ReservaDetalles.AnyAsync(d => d.HabitacionId == id))
                throw new ReglaNegocioException(
                    "No se puede eliminar: tiene reservas. Mejor desactívala (activa = false).");

            _context.Habitaciones.Remove(habitacion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
