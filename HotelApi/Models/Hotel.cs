using System.ComponentModel.DataAnnotations;

namespace HotelApi.Models
{
    public class Hotel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required, MaxLength(60)]
        public string Ciudad { get; set; } = string.Empty;

        public int Estrellas { get; set; } // 1 a 5

        public List<Habitacion> Habitaciones { get; set; } = new();
    }
}
