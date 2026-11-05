namespace HotelApi.Models
{
    public class ReservaDetalle
    {
        public int Id { get; set; }

        public int ReservaId { get; set; }
        public Reserva? Reserva { get; set; }

        public int HabitacionId { get; set; }
        public Habitacion? Habitacion { get; set; }

        public int Noches { get; set; }

        public decimal PrecioPorNoche { get; set; }

        public decimal Subtotal { get; set; }
    }
}
