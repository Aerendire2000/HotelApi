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
    public class ReservasController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ReservasController(HotelDbContext context)
        {
            _context = context;
        }

        // GET api/reservas?usuarioId=1&estado=Pendiente&pagina=1&tamano=10
        [HttpGet]
        public async Task<ActionResult<PaginadoDto<ReservaLecturaDto>>> GetAll(
            [FromQuery] int? usuarioId, [FromQuery] string? estado,
            [FromQuery] int pagina = 1, [FromQuery] int tamano = 10)
        {
            pagina = Math.Max(pagina, 1);
            tamano = Math.Clamp(tamano, 1, 50);

            var consulta = _context.Reservas.AsNoTracking();
            if (usuarioId is not null) consulta = consulta.Where(r => r.UsuarioId == usuarioId);
            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(r => r.Estado == estado);

            var total = await consulta.CountAsync();

            var reservas = await consulta
                .OrderByDescending(r => r.FechaEntrada)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(r => new ReservaLecturaDto
                {
                    Id = r.Id,
                    Usuario = r.Usuario!.NombreCompleto,
                    FechaEntrada = r.FechaEntrada,
                    FechaSalida = r.FechaSalida,
                    Noches = r.FechaSalida.DayNumber - r.FechaEntrada.DayNumber,
                    Estado = r.Estado,
                    CantidadHabitaciones = r.Detalles.Count,
                    Total = r.Total
                })
                .ToListAsync();

            return new PaginadoDto<ReservaLecturaDto>
            {
                Pagina = pagina, TamanoPagina = tamano, TotalRegistros = total, Datos = reservas
            };
        }

        // GET api/reservas/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReservaDetalleDto>> GetById(int id)
        {
            return await ObtenerDetalleAsync(id);
        }

        // POST api/reservas
        [HttpPost]
        public async Task<ActionResult<ReservaDetalleDto>> Create([FromBody] ReservaCrearDto dto)
        {
            var (detalles, servicios) = await ArmarLineasAsync(dto, reservaIdExcluir: 0);

            var reserva = new Reserva
            {
                UsuarioId = dto.UsuarioId,
                FechaEntrada = dto.FechaEntrada!.Value,
                FechaSalida = dto.FechaSalida!.Value,
                Estado = "Pendiente",
                Detalles = detalles,
                Servicios = servicios,
                Total = detalles.Sum(d => d.Subtotal) + servicios.Sum(s => s.Subtotal)
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = reserva.Id },
                await ObtenerDetalleAsync(reserva.Id));
        }

        // PUT api/reservas/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReservaCrearDto dto)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Detalles)
                .Include(r => r.Servicios)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NoEncontradoException($"No existe la reserva con Id {id}.");

            if (reserva.Estado != "Pendiente")
                throw new ReglaNegocioException(
                    "Solo se pueden modificar reservas en estado Pendiente.");

            var (detalles, servicios) = await ArmarLineasAsync(dto, reservaIdExcluir: id);

            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ReservaDetalles.RemoveRange(reserva.Detalles);
                _context.ReservaServicios.RemoveRange(reserva.Servicios);
                await _context.SaveChangesAsync();

                reserva.UsuarioId = dto.UsuarioId;
                reserva.FechaEntrada = dto.FechaEntrada!.Value;
                reserva.FechaSalida = dto.FechaSalida!.Value;
                reserva.Detalles.AddRange(detalles);
                reserva.Servicios.AddRange(servicios);
                reserva.Total = detalles.Sum(d => d.Subtotal) + servicios.Sum(s => s.Subtotal);
                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }

            return NoContent();
        }

        // PATCH api/reservas/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] ReservaEstadoDto dto)
        {
            var reserva = await _context.Reservas.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe la reserva con Id {id}.");

            var permitido = (reserva.Estado, dto.Estado) switch
            {
                ("Pendiente", "Confirmada") => true,
                ("Pendiente", "Cancelada") => true,
                ("Confirmada", "Cancelada") => true,
                _ => false
            };
            if (!permitido)
                throw new ReglaNegocioException(
                    $"No se puede pasar de {reserva.Estado} a {dto.Estado}.");

            reserva.Estado = dto.Estado;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/reservas/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe la reserva con Id {id}.");

            if (reserva.Estado == "Confirmada")
                throw new ReglaNegocioException(
                    "No se puede eliminar una reserva confirmada. Cancélala primero.");

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<ReservaDetalleDto> ObtenerDetalleAsync(int id)
        {
            var reserva = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.Usuario)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Habitacion)
                        .ThenInclude(h => h!.Hotel)
                .Include(r => r.Servicios)
                    .ThenInclude(s => s.ServicioAdicional)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NoEncontradoException($"No existe la reserva con Id {id}.");

            return new ReservaDetalleDto
            {
                Id = reserva.Id,
                FechaCreacion = reserva.FechaCreacion,
                Usuario = reserva.Usuario!.NombreCompleto,
                Email = reserva.Usuario.Email,
                FechaEntrada = reserva.FechaEntrada,
                FechaSalida = reserva.FechaSalida,
                Estado = reserva.Estado,
                Total = reserva.Total,
                Habitaciones = reserva.Detalles.Select(d => new ReservaHabitacionDto
                {
                    Hotel = d.Habitacion!.Hotel!.Nombre,
                    Numero = d.Habitacion.Numero,
                    Tipo = d.Habitacion.Tipo,
                    Noches = d.Noches,
                    PrecioPorNoche = d.PrecioPorNoche,
                    Subtotal = d.Subtotal
                }).ToList(),
                Servicios = reserva.Servicios.Select(s => new ReservaServicioDto
                {
                    Servicio = s.ServicioAdicional!.Nombre,
                    Cantidad = s.Cantidad,
                    PrecioUnitario = s.PrecioUnitario,
                    Subtotal = s.Subtotal
                }).ToList()
            };
        }

        private async Task<(List<ReservaDetalle>, List<ReservaServicio>)> ArmarLineasAsync(
            ReservaCrearDto dto, int reservaIdExcluir)
        {
            var entrada = dto.FechaEntrada!.Value;
            var salida = dto.FechaSalida!.Value;
            var noches = salida.DayNumber - entrada.DayNumber;

            if (!await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId))
                throw new NoEncontradoException($"No existe el usuario con Id {dto.UsuarioId}.");

            var idsHabitaciones = new HashSet<int>();
            foreach (var habitacionId in dto.HabitacionIds)
            {
                if (!idsHabitaciones.Add(habitacionId))
                    throw new SolicitudInvalidaException(
                        $"La habitación {habitacionId} está repetida en la reserva.");
            }

            var habitaciones = await _context.Habitaciones
                .AsNoTracking()
                .Where(h => idsHabitaciones.Contains(h.Id))
                .ToDictionaryAsync(h => h.Id);

            var faltantes = idsHabitaciones.Where(hid => !habitaciones.ContainsKey(hid)).ToList();
            if (faltantes.Count > 0)
                throw new NoEncontradoException(
                    $"No existen las habitaciones: {string.Join(", ", faltantes)}.");

            var inactivas = habitaciones.Values
                .Where(h => !h.Activa).Select(h => h.Numero).ToList();
            if (inactivas.Count > 0)
                throw new ReglaNegocioException(
                    $"Habitaciones fuera de servicio: {string.Join(", ", inactivas)}.");

            var ocupadas = await _context.ReservaDetalles
                .AsNoTracking()
                .Where(d => idsHabitaciones.Contains(d.HabitacionId) &&
                            d.ReservaId != reservaIdExcluir &&
                            d.Reserva!.Estado != "Cancelada" &&
                            d.Reserva.FechaEntrada < salida &&
                            entrada < d.Reserva.FechaSalida)
                .Select(d => d.Habitacion!.Numero)
                .Distinct()
                .ToListAsync();
            if (ocupadas.Count > 0)
                throw new ReglaNegocioException(
                    $"Habitaciones ocupadas en esas fechas: {string.Join(", ", ocupadas)}.");

            var detalles = habitaciones.Values.Select(h => new ReservaDetalle
            {
                HabitacionId = h.Id,
                Noches = noches,
                PrecioPorNoche = h.PrecioPorNoche,
                Subtotal = noches * h.PrecioPorNoche
            }).ToList();

            var cantidades = new Dictionary<int, int>();
            foreach (var s in dto.Servicios)
            {
                if (cantidades.ContainsKey(s.ServicioAdicionalId))
                    cantidades[s.ServicioAdicionalId] += s.Cantidad;
                else
                    cantidades[s.ServicioAdicionalId] = s.Cantidad;
            }

            var idsServicios = cantidades.Keys.ToList();
            var precios = await _context.ServiciosAdicionales
                .AsNoTracking()
                .Where(s => idsServicios.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Precio);

            var serviciosFaltantes = idsServicios.Where(sid => !precios.ContainsKey(sid)).ToList();
            if (serviciosFaltantes.Count > 0)
                throw new NoEncontradoException(
                    $"No existen los servicios: {string.Join(", ", serviciosFaltantes)}.");

            var servicios = cantidades.Select(par => new ReservaServicio
            {
                ServicioAdicionalId = par.Key,
                Cantidad = par.Value,
                PrecioUnitario = precios[par.Key],
                Subtotal = par.Value * precios[par.Key]
            }).ToList();

            return (detalles, servicios);
        }
    }
}
