using Microsoft.EntityFrameworkCore;
using HotelApi.Excepciones;

namespace HotelApi.Middleware
{
    public class ManejoErroresMiddleware
    {
        private readonly RequestDelegate _siguiente;
        private readonly ILogger<ManejoErroresMiddleware> _logger;

        public ManejoErroresMiddleware(
            RequestDelegate siguiente, ILogger<ManejoErroresMiddleware> logger)
        {
            _siguiente = siguiente;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _siguiente(context);
            }
            catch (Exception ex)
            {
                await ResponderErrorAsync(context, ex);
            }
        }

        private async Task ResponderErrorAsync(HttpContext context, Exception ex)
        {
            var (status, titulo, mensaje) = ex switch
            {
                SolicitudInvalidaException => (400, "Solicitud inválida", ex.Message),
                NoEncontradoException => (404, "No encontrado", ex.Message),
                ReglaNegocioException => (409, "Regla de negocio", ex.Message),
                DbUpdateException => (409, "Conflicto en la base de datos",
                    "La operación choca con datos relacionados o duplicados."),
                _ => (500, "Error interno",
                    "Ocurrió un error inesperado. Intenta más tarde.")
            };

            if (status == 500)
                _logger.LogError(ex, "Error no controlado en {Ruta}", context.Request.Path);
            else
                _logger.LogWarning("{Titulo}: {Mensaje}", titulo, ex.Message);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                status,
                titulo,
                mensaje,
                ruta = context.Request.Path.Value,
                hora = DateTime.UtcNow,
                traceId = context.TraceIdentifier
            });
        }
    }
}
