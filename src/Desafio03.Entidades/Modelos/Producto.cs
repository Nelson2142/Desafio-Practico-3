using System.ComponentModel.DataAnnotations;

namespace Desafio03.Entidades.Modelos;

/// <summary>
/// Producto disponible para la venta.
/// </summary>
public class Producto
{
    [Key]
    public int Id { get; set; }

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

    /// <summary>Pedidos en los que aparece el producto (relación 1 a N).</summary>
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
