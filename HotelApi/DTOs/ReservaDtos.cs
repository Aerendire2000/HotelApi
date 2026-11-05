using System.ComponentModel.DataAnnotations;

namespace HotelApi.DTOs
{
    public class ReservaCrearDto : IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "UsuarioId es obligatorio.")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
        public DateOnly? FechaEntrada { get; set; }

        [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
        public DateOnly? FechaSalida { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "La reserva debe tener al menos una habitación.")]
        public List<int> HabitacionIds { get; set; } = new();

        public List<ReservaServicioCrearDto> Servicios { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaEntrada is not null && FechaEntrada < DateOnly.FromDateTime(DateTime.Today))
                yield return new ValidationResult("La fecha de entrada no puede ser pasada.",
                    new[] { nameof(FechaEntrada) });

            if (FechaEntrada is not null && FechaSalida is not null && FechaSalida <= FechaEntrada)
                yield return new ValidationResult(
                    "La fecha de salida debe ser posterior a la de entrada.",
                    new[] { nameof(FechaSalida) });
        }
    }

    public class ReservaServicioCrearDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "ServicioAdicionalId es obligatorio.")]
        public int ServicioAdicionalId { get; set; }

        [Range(1, 20, ErrorMessage = "La cantidad debe estar entre 1 y 20.")]
        public int Cantidad { get; set; } = 1;
    }

    public class ReservaEstadoDto
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [RegularExpression("^(Confirmada|Cancelada)$",
            ErrorMessage = "Estado debe ser Confirmada o Cancelada.")]
        public string Estado { get; set; } = string.Empty;
    }

    public class ReservaLecturaDto
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public DateOnly FechaEntrada { get; set; }
        public DateOnly FechaSalida { get; set; }
        public int Noches { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int CantidadHabitaciones { get; set; }
        public decimal Total { get; set; }
    }

    public class ReservaDetalleDto
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateOnly FechaEntrada { get; set; }
        public DateOnly FechaSalida { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<ReservaHabitacionDto> Habitaciones { get; set; } = new();
        public List<ReservaServicioDto> Servicios { get; set; } = new();
        public decimal Total { get; set; }
    }

    public class ReservaHabitacionDto
    {
        public string Hotel { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Noches { get; set; }
        public decimal PrecioPorNoche { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ReservaServicioDto
    {
        public string Servicio { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
