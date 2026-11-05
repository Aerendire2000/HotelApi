using System.ComponentModel.DataAnnotations;

namespace HotelApi.DTOs
{
    public class HabitacionCrearDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "HotelId es obligatorio.")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "El número es obligatorio.")]
        [RegularExpression(@"^[0-9]{1,4}(-[A-Z])?$",
            ErrorMessage = "Número no válido (ej.: 101 o 202-A).")]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Simple|Doble|Suite)$", ErrorMessage = "Tipo: Simple, Doble o Suite.")]
        public string Tipo { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "El precio por noche debe estar entre 1 y 10000.")]
        public decimal PrecioPorNoche { get; set; }

        [Range(1, 10, ErrorMessage = "La capacidad debe estar entre 1 y 10 personas.")]
        public int Capacidad { get; set; }

        public bool Activa { get; set; } = true;
    }

    public class HabitacionPatchDto
    {
        [RegularExpression("^(Simple|Doble|Suite)$", ErrorMessage = "Tipo: Simple, Doble o Suite.")]
        public string? Tipo { get; set; }

        [Range(1, 10000, ErrorMessage = "El precio por noche debe estar entre 1 y 10000.")]
        public decimal? PrecioPorNoche { get; set; }

        [Range(1, 10, ErrorMessage = "La capacidad debe estar entre 1 y 10 personas.")]
        public int? Capacidad { get; set; }

        public bool? Activa { get; set; }
    }

    public class HabitacionLecturaDto
    {
        public int Id { get; set; }
        public string Hotel { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal PrecioPorNoche { get; set; }
        public int Capacidad { get; set; }
        public bool Activa { get; set; }
    }

    public class HabitacionResumenDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal PrecioPorNoche { get; set; }
        public bool Activa { get; set; }
    }
}
