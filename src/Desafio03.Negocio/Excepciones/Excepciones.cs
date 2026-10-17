namespace Desafio03.Negocio.Excepciones;

/// <summary>
/// El registro solicitado no existe. La API responde 404 Not Found.
/// </summary>
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string recurso, int id)
        : base($"No existe un {recurso} con Id {id}.")
    {
        Recurso = recurso;
        IdBuscado = id;
    }

    public string Recurso { get; }
    public int IdBuscado { get; }
}

/// <summary>
/// Los datos no cumplen las Data Annotations. La API responde 400 Bad Request.
/// </summary>
public class ValidacionException : Exception
{
    public ValidacionException(IDictionary<string, string[]> errores)
        : base("Los datos enviados no son válidos.")
    {
        Errores = new Dictionary<string, string[]>(errores);
    }

    public IReadOnlyDictionary<string, string[]> Errores { get; }
}

/// <summary>
/// Se incumple una regla de negocio (stock insuficiente, email repetido, etc.).
/// La API responde 409 Conflict.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }
}
