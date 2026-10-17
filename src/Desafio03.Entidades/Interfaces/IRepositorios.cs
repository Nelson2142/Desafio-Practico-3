using Desafio03.Entidades.Modelos;

namespace Desafio03.Entidades.Interfaces;

/// <summary>
/// Operaciones de acceso a datos de clientes.
/// Los métodos Agregar/Actualizar/Eliminar solo marcan cambios;
/// se confirman con <see cref="IUnidadDeTrabajo.GuardarCambiosAsync"/>.
/// </summary>
public interface IRepositorioClientes
{
    Task<List<Cliente>> ObtenerTodosAsync();
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteAsync(int id);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
    Task<bool> TienePedidosAsync(int id);
    void Agregar(Cliente cliente);
    void Actualizar(Cliente cliente);
    void Eliminar(Cliente cliente);
}

/// <summary>
/// Operaciones de acceso a datos de productos.
/// </summary>
public interface IRepositorioProductos
{
    Task<List<Producto>> ObtenerTodosAsync();
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<bool> TienePedidosAsync(int id);
    void Agregar(Producto producto);
    void Actualizar(Producto producto);
    void Eliminar(Producto producto);
}

/// <summary>
/// Operaciones de acceso a datos de pedidos.
/// Las consultas incluyen el cliente y el producto relacionados.
/// </summary>
public interface IRepositorioPedidos
{
    Task<List<Pedido>> ObtenerTodosAsync();
    Task<Pedido?> ObtenerPorIdAsync(int id);
    Task<List<Pedido>> ObtenerPorClienteAsync(int clienteId);
    void Agregar(Pedido pedido);
    void Actualizar(Pedido pedido);
    void Eliminar(Pedido pedido);
}

/// <summary>
/// Agrupa los repositorios y confirma todos los cambios en una sola operación
/// (por ejemplo: crear un pedido y descontar el stock del producto).
/// </summary>
public interface IUnidadDeTrabajo
{
    IRepositorioClientes Clientes { get; }
    IRepositorioProductos Productos { get; }
    IRepositorioPedidos Pedidos { get; }
    Task<int> GuardarCambiosAsync();
}
