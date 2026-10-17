using System.ComponentModel.DataAnnotations;
using Desafio03.Entidades.Modelos;

namespace Desafio03.Negocio.DTOs;

/// <summary>Datos para crear o actualizar un cliente.</summary>
public class ClienteGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(Patrones.SoloLetras, ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(Patrones.SoloLetras, ErrorMessage = "El apellido solo puede contener letras y espacios.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [StringLength(100, ErrorMessage = "El email no puede superar los 100 caracteres.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [RegularExpression(Patrones.Email, ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>Cliente devuelto por la API.</summary>
public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombre} {Apellido}";

    public static ClienteDto DesdeEntidad(Cliente c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Apellido = c.Apellido,
        Email = c.Email
    };
}
