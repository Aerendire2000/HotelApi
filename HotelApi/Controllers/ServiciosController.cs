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
    public class ServiciosController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ServiciosController(HotelDbContext context)
        {
            _context = context;
        }

        // GET api/servicios
        [HttpGet]
        public async Task<ActionResult<List<ServicioLecturaDto>>> GetAll()
        {
            return await _context.ServiciosAdicionales
                .AsNoTracking()
                .OrderBy(s => s.Nombre)
                .Select(s => new ServicioLecturaDto
                {
                    Id = s.Id, Nombre = s.Nombre, Descripcion = s.Descripcion, Precio = s.Precio
                })
                .ToListAsync();
        }

        // GET api/servicios/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServicioLecturaDto>> GetById(int id)
        {
            var servicio = await _context.ServiciosAdicionales
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new ServicioLecturaDto
                {
                    Id = s.Id, Nombre = s.Nombre, Descripcion = s.Descripcion, Precio = s.Precio
                })
                .FirstOrDefaultAsync();

            return servicio
                ?? throw new NoEncontradoException($"No existe el servicio con Id {id}.");
        }

        // POST api/servicios
        [HttpPost]
        public async Task<ActionResult<ServicioLecturaDto>> Create([FromBody] ServicioCrearDto dto)
        {
            var servicio = new ServicioAdicional
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion,
                Precio = dto.Precio
            };

            _context.ServiciosAdicionales.Add(servicio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = servicio.Id }, new ServicioLecturaDto
            {
                Id = servicio.Id, Nombre = servicio.Nombre,
                Descripcion = servicio.Descripcion, Precio = servicio.Precio
            });
        }

        // PUT api/servicios/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServicioCrearDto dto)
        {
            var servicio = await _context.ServiciosAdicionales.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el servicio con Id {id}.");

            servicio.Nombre = dto.Nombre.Trim();
            servicio.Descripcion = dto.Descripcion;
            servicio.Precio = dto.Precio;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/servicios/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] ServicioPatchDto dto)
        {
            var servicio = await _context.ServiciosAdicionales.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el servicio con Id {id}.");

            if (dto.Nombre is not null) servicio.Nombre = dto.Nombre.Trim();
            if (dto.Descripcion is not null) servicio.Descripcion = dto.Descripcion;
            if (dto.Precio is not null) servicio.Precio = dto.Precio.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/servicios/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var servicio = await _context.ServiciosAdicionales.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el servicio con Id {id}.");

            if (await _context.ReservaServicios.AnyAsync(rs => rs.ServicioAdicionalId == id))
                throw new ReglaNegocioException(
                    "No se puede eliminar: el servicio está en reservas.");

            _context.ServiciosAdicionales.Remove(servicio);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
