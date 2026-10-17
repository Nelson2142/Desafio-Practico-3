using System.ComponentModel.DataAnnotations;
using Desafio03.Negocio.Excepciones;

namespace Desafio03.Negocio.Validacion;

/// <summary>
/// Ejecuta las Data Annotations de un objeto desde la capa de negocio.
/// Así las validaciones se cumplen aunque el servicio no se use desde la API.
/// </summary>
public static class Validador
{
    /// <summary>Devuelve los errores agrupados por propiedad (vacío si es válido).</summary>
    public static Dictionary<string, string[]> ObtenerErrores(object instancia)
    {
        ArgumentNullException.ThrowIfNull(instancia);

        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(instancia, new ValidationContext(instancia), resultados, validateAllProperties: true);

        return resultados
            .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : new[] { string.Empty })
                .Select(miembro => (miembro, mensaje: r.ErrorMessage ?? "Valor no válido.")))
            .GroupBy(e => e.miembro)
            .ToDictionary(g => g.Key, g => g.Select(e => e.mensaje).Distinct().ToArray());
    }

    /// <summary>Lanza <see cref="ValidacionException"/> si el objeto no es válido.</summary>
    public static void Validar(object instancia)
    {
        var errores = ObtenerErrores(instancia);
        if (errores.Count > 0)
        {
            throw new ValidacionException(errores);
        }
    }
}
