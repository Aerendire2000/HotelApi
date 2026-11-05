using System.ComponentModel.DataAnnotations;

namespace HotelApi.DTOs
{
    public class ServicioCrearDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Descripcion { get; set; }

        [Range(0.5, 5000, ErrorMessage = "El precio debe estar entre 0.50 y 5000.")]
        public decimal Precio { get; set; }
    }

    public class ServicioPatchDto
    {
        [StringLength(80, MinimumLength = 3)]
        public string? Nombre { get; set; }

        [StringLength(200)]
        public string? Descripcion { get; set; }

        [Range(0.5, 5000, ErrorMessage = "El precio debe estar entre 0.50 y 5000.")]
        public decimal? Precio { get; set; }
    }

    public class ServicioLecturaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
    }
}
