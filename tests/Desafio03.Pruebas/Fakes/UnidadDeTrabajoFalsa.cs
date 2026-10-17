using Desafio03.Entidades.Interfaces;
using Desafio03.Entidades.Modelos;

namespace Desafio03.Pruebas.Fakes;

/// <summary>
/// Implementación en memoria de la unidad de trabajo para las pruebas unitarias.
/// Reemplaza a la base de datos: así se prueba solo la lógica de negocio.
/// </summary>
public class UnidadDeTrabajoFalsa : IUnidadDeTrabajo
{
    public List<Cliente> TablaClientes { get; } = new();
    public List<Producto> TablaProductos { get; } = new();
    public List<Pedido> TablaPedidos { get; } = new();

    /// <summary>Veces que se llamó a GuardarCambiosAsync.</summary>
    public int GuardadosRealizados { get; private set; }

    public UnidadDeTrabajoFalsa()
    {
        Clientes = new RepositorioClientesFalso(this);
        Productos = new RepositorioProductosFalso(this);
        Pedidos = new RepositorioPedidosFalso(this);
    }

    public IRepositorioClientes Clientes { get; }
    public IRepositorioProductos Productos { get; }
    public IRepositorioPedidos Pedidos { get; }

    public Task<int> GuardarCambiosAsync()
    {
        GuardadosRealizados++;
        return Task.FromResult(1);
    }

    // ----- Atajos para preparar datos en las pruebas -----

    public Cliente AgregarCliente(string nombre, string apellido, string email)
    {
        var cliente = new Cliente { Nombre = nombre, Apellido = apellido, Email = email };
        Clientes.Agregar(cliente);
        return cliente;
    }

    public Producto AgregarProducto(string nombre, decimal precio, int stock)
    {
        var producto = new Producto { Nombre = nombre, Descripcion = $"Descripción de {nombre}", Precio = precio, Stock = stock };
        Productos.Agregar(producto);
        return producto;
    }

    private static int SiguienteId<T>(List<T> tabla, Func<T, int> id) =>
        tabla.Count == 0 ? 1 : tabla.Max(id) + 1;

    private sealed class RepositorioClientesFalso : IRepositorioClientes
    {
        private readonly UnidadDeTrabajoFalsa _uow;
        public RepositorioClientesFalso(UnidadDeTrabajoFalsa uow) => _uow = uow;

        public Task<List<Cliente>> ObtenerTodosAsync() => Task.FromResult(_uow.TablaClientes.ToList());

        public Task<Cliente?> ObtenerPorIdAsync(int id) =>
            Task.FromResult(_uow.TablaClientes.FirstOrDefault(c => c.Id == id));

        public Task<bool> ExisteAsync(int id) => Task.FromResult(_uow.TablaClientes.Any(c => c.Id == id));

        public Task<bool> ExisteEmailAsync(string email, int? excluirId = null) =>
            Task.FromResult(_uow.TablaClientes.Any(c =>
                string.Equals(c.Email, email.Trim(), StringComparison.OrdinalIgnoreCase) &&
                (excluirId == null || c.Id != excluirId)));

        public Task<bool> TienePedidosAsync(int id) => Task.FromResult(_uow.TablaPedidos.Any(p => p.ClienteId == id));

        public void Agregar(Cliente cliente)
        {
            cliente.Id = SiguienteId(_uow.TablaClientes, c => c.Id);
            _uow.TablaClientes.Add(cliente);
        }

        public void Actualizar(Cliente cliente) { }

        public void Eliminar(Cliente cliente) => _uow.TablaClientes.Remove(cliente);
    }

    private sealed class RepositorioProductosFalso : IRepositorioProductos
    {
        private readonly UnidadDeTrabajoFalsa _uow;
        public RepositorioProductosFalso(UnidadDeTrabajoFalsa uow) => _uow = uow;

        public Task<List<Producto>> ObtenerTodosAsync() => Task.FromResult(_uow.TablaProductos.ToList());

        public Task<Producto?> ObtenerPorIdAsync(int id) =>
            Task.FromResult(_uow.TablaProductos.FirstOrDefault(p => p.Id == id));

        public Task<bool> TienePedidosAsync(int id) => Task.FromResult(_uow.TablaPedidos.Any(p => p.ProductoId == id));

        public void Agregar(Producto producto)
        {
            producto.Id = SiguienteId(_uow.TablaProductos, p => p.Id);
            _uow.TablaProductos.Add(producto);
        }

        public void Actualizar(Producto producto) { }

        public void Eliminar(Producto producto) => _uow.TablaProductos.Remove(producto);
    }

    private sealed class RepositorioPedidosFalso : IRepositorioPedidos
    {
        private readonly UnidadDeTrabajoFalsa _uow;
        public RepositorioPedidosFalso(UnidadDeTrabajoFalsa uow) => _uow = uow;

        public Task<List<Pedido>> ObtenerTodosAsync() => Task.FromResult(_uow.TablaPedidos.ToList());

        public Task<Pedido?> ObtenerPorIdAsync(int id) =>
            Task.FromResult(_uow.TablaPedidos.FirstOrDefault(p => p.Id == id));

        public Task<List<Pedido>> ObtenerPorClienteAsync(int clienteId) =>
            Task.FromResult(_uow.TablaPedidos.Where(p => p.ClienteId == clienteId).ToList());

        public void Agregar(Pedido pedido)
        {
            pedido.Id = SiguienteId(_uow.TablaPedidos, p => p.Id);
            _uow.TablaPedidos.Add(pedido);
        }

        public void Actualizar(Pedido pedido) { }

        public void Eliminar(Pedido pedido) => _uow.TablaPedidos.Remove(pedido);
    }
}
