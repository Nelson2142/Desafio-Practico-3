using Desafio03.AccesoDatos.Contexto;
using Desafio03.Entidades.Interfaces;
using Desafio03.Entidades.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Desafio03.AccesoDatos.Repositorios;

/// <summary>
/// Utilidad común: las entidades obtenidas con ObtenerPorIdAsync ya están siendo
/// rastreadas por EF Core (sus cambios se detectan solos). Solo se adjuntan como
/// modificadas las que llegan desconectadas del contexto.
/// </summary>
internal static class ExtensionesContexto
{
    public static void MarcarModificado<T>(this PedidosDbContext contexto, T entidad) where T : class
    {
        if (contexto.Entry(entidad).State == EntityState.Detached)
        {
            contexto.Set<T>().Update(entidad);
        }
    }
}

public class RepositorioClientes : IRepositorioClientes
{
    private readonly PedidosDbContext _contexto;

    public RepositorioClientes(PedidosDbContext contexto) => _contexto = contexto;

    public Task<List<Cliente>> ObtenerTodosAsync() =>
        _contexto.Clientes.AsNoTracking().OrderBy(c => c.Apellido).ThenBy(c => c.Nombre).ToListAsync();

    public Task<Cliente?> ObtenerPorIdAsync(int id) =>
        _contexto.Clientes.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExisteAsync(int id) =>
        _contexto.Clientes.AnyAsync(c => c.Id == id);

    public Task<bool> ExisteEmailAsync(string email, int? excluirId = null)
    {
        var normalizado = email.Trim().ToLower();
        return _contexto.Clientes.AnyAsync(c =>
            c.Email.ToLower() == normalizado && (excluirId == null || c.Id != excluirId));
    }

    public Task<bool> TienePedidosAsync(int id) =>
        _contexto.Pedidos.AnyAsync(p => p.ClienteId == id);

    public void Agregar(Cliente cliente) => _contexto.Clientes.Add(cliente);

    public void Actualizar(Cliente cliente) => _contexto.MarcarModificado(cliente);

    public void Eliminar(Cliente cliente) => _contexto.Clientes.Remove(cliente);
}

public class RepositorioProductos : IRepositorioProductos
{
    private readonly PedidosDbContext _contexto;

    public RepositorioProductos(PedidosDbContext contexto) => _contexto = contexto;

    public Task<List<Producto>> ObtenerTodosAsync() =>
        _contexto.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();

    public Task<Producto?> ObtenerPorIdAsync(int id) =>
        _contexto.Productos.FirstOrDefaultAsync(p => p.Id == id);

    public Task<bool> TienePedidosAsync(int id) =>
        _contexto.Pedidos.AnyAsync(p => p.ProductoId == id);

    public void Agregar(Producto producto) => _contexto.Productos.Add(producto);

    public void Actualizar(Producto producto) => _contexto.MarcarModificado(producto);

    public void Eliminar(Producto producto) => _contexto.Productos.Remove(producto);
}

public class RepositorioPedidos : IRepositorioPedidos
{
    private readonly PedidosDbContext _contexto;

    public RepositorioPedidos(PedidosDbContext contexto) => _contexto = contexto;

    private IQueryable<Pedido> ConRelaciones() =>
        _contexto.Pedidos.Include(p => p.Cliente).Include(p => p.Producto);

    public Task<List<Pedido>> ObtenerTodosAsync() =>
        ConRelaciones().AsNoTracking().OrderByDescending(p => p.FechaPedido).ToListAsync();

    public Task<Pedido?> ObtenerPorIdAsync(int id) =>
        ConRelaciones().FirstOrDefaultAsync(p => p.Id == id);

    public Task<List<Pedido>> ObtenerPorClienteAsync(int clienteId) =>
        ConRelaciones().AsNoTracking()
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();

    public void Agregar(Pedido pedido) => _contexto.Pedidos.Add(pedido);

    public void Actualizar(Pedido pedido) => _contexto.MarcarModificado(pedido);

    public void Eliminar(Pedido pedido) => _contexto.Pedidos.Remove(pedido);
}

/// <summary>
/// Unidad de trabajo: todos los repositorios comparten el mismo DbContext,
/// por lo que GuardarCambiosAsync confirma todo en una sola transacción.
/// </summary>
public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly PedidosDbContext _contexto;

    public UnidadDeTrabajo(PedidosDbContext contexto)
    {
        _contexto = contexto;
        Clientes = new RepositorioClientes(contexto);
        Productos = new RepositorioProductos(contexto);
        Pedidos = new RepositorioPedidos(contexto);
    }

    public IRepositorioClientes Clientes { get; }
    public IRepositorioProductos Productos { get; }
    public IRepositorioPedidos Pedidos { get; }

    public Task<int> GuardarCambiosAsync() => _contexto.SaveChangesAsync();
}
