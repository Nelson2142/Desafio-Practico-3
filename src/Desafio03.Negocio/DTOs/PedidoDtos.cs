using System.ComponentModel.DataAnnotations;
using Desafio03.Entidades.Modelos;

namespace Desafio03.Negocio.DTOs;

/// <summary>Datos para crear o actualizar un pedido.</summary>
public class PedidoGuardarDto
{
    /// <summary>Si no se envía, se usa la fecha y hora actual.</summary>
    [DataType(DataType.DateTime)]
    public DateTime? FechaPedido { get; set; }

    [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1,000 unidades.")]
    public int Cantidad { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un ClienteId válido.")]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un ProductoId válido.")]
    public int ProductoId { get; set; }
}

/// <summary>Pedido devuelto por la API, con los nombres del cliente y producto.</summary>
public class PedidoDto
{
    public int Id { get; set; }
    public DateTime FechaPedido { get; set; }
    public int Cantidad { get; set; }
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }

    public static PedidoDto DesdeEntidad(Pedido p) => new()
    {
        Id = p.Id,
        FechaPedido = p.FechaPedido,
        Cantidad = p.Cantidad,
        ClienteId = p.ClienteId,
        Cliente = p.Cliente is null ? string.Empty : $"{p.Cliente.Nombre} {p.Cliente.Apellido}",
        ProductoId = p.ProductoId,
        Producto = p.Producto?.Nombre ?? string.Empty,
        PrecioUnitario = p.PrecioUnitario,
        Total = p.Total
    };
}
