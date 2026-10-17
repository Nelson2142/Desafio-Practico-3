using System.ComponentModel.DataAnnotations;

namespace Desafio03.Entidades.Modelos;

/// <summary>
/// Pedido de un producto realizado por un cliente.
/// </summary>
public class Pedido
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha del pedido es obligatoria.")]
    [DataType(DataType.DateTime)]
    public DateTime FechaPedido { get; set; }

    [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1,000 unidades.")]
    public int Cantidad { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El cliente es obligatorio.")]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El producto es obligatorio.")]
    public int ProductoId { get; set; }

    /// <summary>Precio del producto al momento de realizar el pedido.</summary>
    [Range(0.01, 999999.99)]
    public decimal PrecioUnitario { get; set; }

    /// <summary>PrecioUnitario × Cantidad.</summary>
    [Range(0.01, 999999999.99)]
    public decimal Total { get; set; }

    public Cliente? Cliente { get; set; }

    public Producto? Producto { get; set; }
}
