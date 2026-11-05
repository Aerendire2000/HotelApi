using System.ComponentModel.DataAnnotations;

namespace HotelApi.DTOs
{
    public class UsuarioCrearDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Email no válido.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^9[0-9]{8}$",
            ErrorMessage = "El teléfono debe tener 9 dígitos y empezar con 9.")]
        public string? Telefono { get; set; }
    }

    public class UsuarioPatchDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? NombreCompleto { get; set; }

        [EmailAddress(ErrorMessage = "Email no válido.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [RegularExpression(@"^9[0-9]{8}$",
            ErrorMessage = "El teléfono debe tener 9 dígitos y empezar con 9.")]
        public string? Telefono { get; set; }
    }

    public class UsuarioLecturaDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public int TotalReservas { get; set; }
    }
}
