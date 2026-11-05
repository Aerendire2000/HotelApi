namespace HotelApi.Models
{
    public class ReservaServicio
    {
        public int Id { get; set; }

        public int ReservaId { get; set; }
        public Reserva? Reserva { get; set; }

        public int ServicioAdicionalId { get; set; }
        public ServicioAdicional? ServicioAdicional { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
