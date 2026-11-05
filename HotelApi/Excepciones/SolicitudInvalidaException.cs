namespace HotelApi.Excepciones
{
    public class SolicitudInvalidaException : Exception
    {
        public SolicitudInvalidaException(string mensaje) : base(mensaje) { }
    }
}
