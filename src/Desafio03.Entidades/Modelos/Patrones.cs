namespace Desafio03.Entidades.Modelos;

/// <summary>
/// Expresiones regulares compartidas por las validaciones.
/// </summary>
public static class Patrones
{
    /// <summary>Letras (incluye tildes, ñ y ü) y espacios.</summary>
    public const string SoloLetras = @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s]+$";

    /// <summary>
    /// Email con usuario, @, dominio y extensión (ej. nombre@dominio.com).
    /// Complementa a [EmailAddress], que acepta valores como "a@b".
    /// </summary>
    public const string Email = @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$";
}
