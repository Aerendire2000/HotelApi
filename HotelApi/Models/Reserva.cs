using System.ComponentModel.DataAnnotations;

namespace HotelApi.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateOnly FechaEntrada { get; set; }

        public DateOnly FechaSalida { get; set; }

        [Required, MaxLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmada, Cancelada

        public decimal Total { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public List<ReservaDetalle> Detalles { get; set; } = new();

        public List<ReservaServicio> Servicios { get; set; } = new();
    }
}
