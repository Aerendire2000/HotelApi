using System.ComponentModel.DataAnnotations;

namespace HotelApi.Models
{
    public class Habitacion
    {
        public int Id { get; set; }

        [Required, MaxLength(10)]
        public string Numero { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Tipo { get; set; } = string.Empty; // Simple, Doble, Suite

        public decimal PrecioPorNoche { get; set; }

        public int Capacidad { get; set; } // personas

        public bool Activa { get; set; } = true;

        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }
    }
}
