using System.ComponentModel.DataAnnotations;

namespace HotelApi.Models
{
    public class ServicioAdicional
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }
    }
}
