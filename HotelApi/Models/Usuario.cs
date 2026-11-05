using System.ComponentModel.DataAnnotations;

namespace HotelApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(9)]
        public string? Telefono { get; set; }

        public DateTime FechaRegistro { get; set; }

        public List<Reserva> Reservas { get; set; } = new();
    }
}
