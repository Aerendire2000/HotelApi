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
    public class UsuariosController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public UsuariosController(HotelDbContext context)
        {
            _context = context;
        }

        // GET api/usuarios
        [HttpGet]
        public async Task<ActionResult<List<UsuarioLecturaDto>>> GetAll()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new UsuarioLecturaDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    TotalReservas = u.Reservas.Count
                })
                .ToListAsync();
        }

        // GET api/usuarios/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioLecturaDto>> GetById(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UsuarioLecturaDto
                {
                    Id = u.Id, NombreCompleto = u.NombreCompleto, Email = u.Email,
                    Telefono = u.Telefono, TotalReservas = u.Reservas.Count
                })
                .FirstOrDefaultAsync();

            return usuario ?? throw new NoEncontradoException($"No existe el usuario con Id {id}.");
        }

        // POST api/usuarios
        [HttpPost]
        public async Task<ActionResult<UsuarioLecturaDto>> Create([FromBody] UsuarioCrearDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            if (await _context.Usuarios.AnyAsync(u => u.Email == email))
                throw new ReglaNegocioException($"Ya existe un usuario con el email {email}.");

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto.Trim(),
                Email = email,
                Telefono = dto.Telefono
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, new UsuarioLecturaDto
            {
                Id = usuario.Id, NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email, Telefono = usuario.Telefono
            });
        }

        // PUT api/usuarios/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioCrearDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el usuario con Id {id}.");

            var email = dto.Email.Trim().ToLower();
            if (await _context.Usuarios.AnyAsync(u => u.Email == email && u.Id != id))
                throw new ReglaNegocioException($"Ya existe otro usuario con el email {email}.");

            usuario.NombreCompleto = dto.NombreCompleto.Trim();
            usuario.Email = email;
            usuario.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/usuarios/1
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] UsuarioPatchDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el usuario con Id {id}.");

            if (dto.Email is not null)
            {
                var email = dto.Email.Trim().ToLower();
                if (await _context.Usuarios.AnyAsync(u => u.Email == email && u.Id != id))
                    throw new ReglaNegocioException(
                        $"Ya existe otro usuario con el email {email}.");
                usuario.Email = email;
            }
            if (dto.NombreCompleto is not null) usuario.NombreCompleto = dto.NombreCompleto.Trim();
            if (dto.Telefono is not null) usuario.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/usuarios/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id)
                ?? throw new NoEncontradoException($"No existe el usuario con Id {id}.");

            if (await _context.Reservas.AnyAsync(r => r.UsuarioId == id))
                throw new ReglaNegocioException("No se puede eliminar: el usuario tiene reservas.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
