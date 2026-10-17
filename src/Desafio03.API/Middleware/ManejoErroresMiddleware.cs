using System.Net;
using System.Text.Json;
using Desafio03.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace Desafio03.API.Middleware;

/// <summary>
/// Convierte las excepciones de la capa de negocio en respuestas HTTP:
///  - RecursoNoEncontradoException → 404
///  - ValidacionException          → 400 (con el detalle por campo)
///  - ReglaNegocioException        → 409
///  - Cualquier otra               → 500
/// </summary>
public class ManejoErroresMiddleware
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejoErroresMiddleware> _logger;

    public ManejoErroresMiddleware(RequestDelegate siguiente, ILogger<ManejoErroresMiddleware> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (Exception ex)
        {
            await EscribirErrorAsync(contexto, ex);
        }
    }

    private async Task EscribirErrorAsync(HttpContext contexto, Exception ex)
    {
        var (estado, titulo, errores) = ex switch
        {
            RecursoNoEncontradoException => (HttpStatusCode.NotFound, "Recurso no encontrado", null),
            ValidacionException v => (HttpStatusCode.BadRequest, "Datos no válidos", v.Errores),
            ReglaNegocioException => (HttpStatusCode.Conflict, "Regla de negocio incumplida", null),
            DbUpdateException => (HttpStatusCode.Conflict, "No se pudo guardar en la base de datos", null),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor", (IReadOnlyDictionary<string, string[]>?)null)
        };

        if (estado == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(ex, "Error no controlado en {Ruta}", contexto.Request.Path);
        }

        var detalle = estado switch
        {
            HttpStatusCode.InternalServerError => "Ocurrió un error inesperado. Revise los logs de la API.",
            _ when ex is DbUpdateException => ex.InnerException?.Message ?? ex.Message,
            _ => ex.Message
        };

        var respuesta = new
        {
            status = (int)estado,
            title = titulo,
            detail = detalle,
            errors = errores,
            traceId = contexto.TraceIdentifier
        };

        contexto.Response.Clear();
        contexto.Response.StatusCode = (int)estado;
        contexto.Response.ContentType = "application/problem+json";
        await contexto.Response.WriteAsync(JsonSerializer.Serialize(respuesta, OpcionesJson));
    }
}
