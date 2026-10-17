using Desafio03.Entidades.Interfaces;
using Desafio03.Entidades.Modelos;
using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Excepciones;
using Desafio03.Negocio.Validacion;

namespace Desafio03.Negocio.Servicios;

public interface IPedidoServicio
{
    Task<List<PedidoDto>> ObtenerTodosAsync();
    Task<PedidoDto> ObtenerPorIdAsync(int id);
    Task<PedidoDto> CrearAsync(PedidoGuardarDto datos);
    Task<PedidoDto> ActualizarAsync(int id, PedidoGuardarDto datos);
    Task EliminarAsync(int id);
}

/// <summary>
/// Lógica de negocio de pedidos.
/// Reglas:
///  1. El cliente y el producto deben existir.
///  2. La cantidad no puede superar el stock disponible.
///  3. Al crear un pedido se descuenta el stock; al eliminarlo se devuelve.
///  4. La fecha del pedido no puede ser futura.
///  5. El total se calcula como precio unitario × cantidad.
/// </summary>
public class PedidoServicio : IPedidoServicio
{
    /// <summary>Margen para diferencias de reloj entre cliente y servidor.</summary>
    private static readonly TimeSpan ToleranciaFecha = TimeSpan.FromMinutes(5);

    private readonly IUnidadDeTrabajo _uow;

    public PedidoServicio(IUnidadDeTrabajo unidadDeTrabajo)
    {
        _uow = unidadDeTrabajo;
    }

    public async Task<List<PedidoDto>> ObtenerTodosAsync()
    {
        var pedidos = await _uow.Pedidos.ObtenerTodosAsync();
        return pedidos.Select(PedidoDto.DesdeEntidad).ToList();
    }

    public async Task<PedidoDto> ObtenerPorIdAsync(int id)
    {
        var pedido = await BuscarAsync(id);
        return PedidoDto.DesdeEntidad(pedido);
    }

    public async Task<PedidoDto> CrearAsync(PedidoGuardarDto datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        Validador.Validar(datos);

        var fecha = ValidarFecha(datos.FechaPedido);
        var cliente = await ObtenerClienteAsync(datos.ClienteId);
        var producto = await ObtenerProductoAsync(datos.ProductoId);

        ValidarStock(producto, disponible: producto.Stock, solicitado: datos.Cantidad);

        producto.Stock -= datos.Cantidad;
        _uow.Productos.Actualizar(producto);

        var pedido = new Pedido
        {
            FechaPedido = fecha,
            Cantidad = datos.Cantidad,
            ClienteId = cliente.Id,
            ProductoId = producto.Id,
            PrecioUnitario = producto.Precio,
            Total = producto.Precio * datos.Cantidad,
            Cliente = cliente,
            Producto = producto
        };

        Validador.Validar(pedido);
        _uow.Pedidos.Agregar(pedido);

        // Pedido y descuento de stock se guardan juntos en una sola operación.
        await _uow.GuardarCambiosAsync();

        return PedidoDto.DesdeEntidad(pedido);
    }

    public async Task<PedidoDto> ActualizarAsync(int id, PedidoGuardarDto datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        Validador.Validar(datos);

        var pedido = await BuscarAsync(id);
        var fecha = ValidarFecha(datos.FechaPedido ?? pedido.FechaPedido);
        var cliente = await ObtenerClienteAsync(datos.ClienteId);
        var productoNuevo = await ObtenerProductoAsync(datos.ProductoId);
        var mismoProducto = productoNuevo.Id == pedido.ProductoId;

        // Si es el mismo producto, las unidades del pedido actual vuelven a estar disponibles.
        var disponible = productoNuevo.Stock + (mismoProducto ? pedido.Cantidad : 0);
        ValidarStock(productoNuevo, disponible, datos.Cantidad);

        // Se devuelve el stock del pedido original y se descuenta el nuevo.
        var productoAnterior = mismoProducto
            ? productoNuevo
            : pedido.Producto ?? await ObtenerProductoAsync(pedido.ProductoId);

        productoAnterior.Stock += pedido.Cantidad;
        productoNuevo.Stock -= datos.Cantidad;
        _uow.Productos.Actualizar(productoAnterior);
        if (!mismoProducto)
        {
            _uow.Productos.Actualizar(productoNuevo);
        }

        pedido.FechaPedido = fecha;
        pedido.Cantidad = datos.Cantidad;
        pedido.ClienteId = cliente.Id;
        pedido.Cliente = cliente;
        pedido.ProductoId = productoNuevo.Id;
        pedido.Producto = productoNuevo;
        if (!mismoProducto)
        {
            pedido.PrecioUnitario = productoNuevo.Precio;
        }
        pedido.Total = pedido.PrecioUnitario * pedido.Cantidad;

        Validador.Validar(pedido);
        _uow.Pedidos.Actualizar(pedido);
        await _uow.GuardarCambiosAsync();

        return PedidoDto.DesdeEntidad(pedido);
    }

    public async Task EliminarAsync(int id)
    {
        var pedido = await BuscarAsync(id);

        // Al cancelar el pedido, las unidades regresan al inventario.
        var producto = pedido.Producto ?? await _uow.Productos.ObtenerPorIdAsync(pedido.ProductoId);
        if (producto is not null)
        {
            producto.Stock += pedido.Cantidad;
            _uow.Productos.Actualizar(producto);
        }

        _uow.Pedidos.Eliminar(pedido);
        await _uow.GuardarCambiosAsync();
    }

    private async Task<Pedido> BuscarAsync(int id)
    {
        return await _uow.Pedidos.ObtenerPorIdAsync(id)
               ?? throw new RecursoNoEncontradoException("pedido", id);
    }

    private async Task<Cliente> ObtenerClienteAsync(int clienteId)
    {
        return await _uow.Clientes.ObtenerPorIdAsync(clienteId)
               ?? throw new ReglaNegocioException($"El cliente con Id {clienteId} no existe.");
    }

    private async Task<Producto> ObtenerProductoAsync(int productoId)
    {
        return await _uow.Productos.ObtenerPorIdAsync(productoId)
               ?? throw new ReglaNegocioException($"El producto con Id {productoId} no existe.");
    }

    private static void ValidarStock(Producto producto, int disponible, int solicitado)
    {
        if (solicitado > disponible)
        {
            throw new ReglaNegocioException(
                $"Stock insuficiente para '{producto.Nombre}'. Disponible: {disponible}, solicitado: {solicitado}.");
        }
    }

    private static DateTime ValidarFecha(DateTime? fecha)
    {
        var valor = fecha ?? DateTime.Now;

        // Swagger envía las fechas en UTC (terminan en "Z"); se pasan a la hora local
        // del servidor para compararlas correctamente.
        if (valor.Kind == DateTimeKind.Utc)
        {
            valor = valor.ToLocalTime();
        }

        if (valor > DateTime.Now.Add(ToleranciaFecha))
        {
            throw new ReglaNegocioException("La fecha del pedido no puede ser posterior a la fecha actual.");
        }

        return valor;
    }
}
