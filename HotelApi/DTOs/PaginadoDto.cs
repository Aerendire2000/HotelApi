namespace HotelApi.DTOs
{
    public class PaginadoDto<T>
    {
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas => (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
        public List<T> Datos { get; set; } = new();
    }
}
