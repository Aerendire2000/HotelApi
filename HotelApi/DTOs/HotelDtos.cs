using System.ComponentModel.DataAnnotations;

namespace HotelApi.DTOs
{
    public class HotelCrearDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        [StringLength(60)]
        public string Ciudad { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Las estrellas van de 1 a 5.")]
        public int Estrellas { get; set; }
    }

    public class HotelPatchDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Nombre { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(60)]
        public string? Ciudad { get; set; }

        [Range(1, 5, ErrorMessage = "Las estrellas van de 1 a 5.")]
        public int? Estrellas { get; set; }
    }

    public class HotelLecturaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public int Estrellas { get; set; }
        public int TotalHabitaciones { get; set; }
        public decimal? PrecioDesde { get; set; }
    }

    public class HotelDetalleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public int Estrellas { get; set; }
        public List<HabitacionResumenDto> Habitaciones { get; set; } = new();
    }
}
