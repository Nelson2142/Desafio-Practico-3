using System.ComponentModel.DataAnnotations;
using Desafio03.Entidades.Modelos;

namespace Desafio03.Negocio.DTOs;

/// <summary>Datos para crear o actualizar un producto.</summary>
public class ProductoGuardarDto
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "La descripción debe tener entre 5 y 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99.")]
    public decimal Precio { get; set; }

    [Range(0, 100000, ErrorMessage = "El stock debe estar entre 0 y 100,000 unidades.")]
    public int Stock { get; set; }
}

/// <summary>Producto devuelto por la API.</summary>
public class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }

    public static ProductoDto DesdeEntidad(Producto p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        Precio = p.Precio,
        Stock = p.Stock
    };
}
