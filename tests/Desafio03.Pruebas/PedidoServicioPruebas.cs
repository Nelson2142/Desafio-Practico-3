using Desafio03.Entidades.Modelos;
using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Excepciones;
using Desafio03.Negocio.Servicios;
using Desafio03.Pruebas.Fakes;

namespace Desafio03.Pruebas;

public class PedidoServicioPruebas
{
    private readonly UnidadDeTrabajoFalsa _uow = new();
    private readonly PedidoServicio _servicio;
    private readonly Cliente _cliente;
    private readonly Producto _producto;

    public PedidoServicioPruebas()
    {
        _servicio = new PedidoServicio(_uow);
        _cliente = _uow.AgregarCliente("Nelson", "Rivas", "nelson@correo.com");
        _producto = _uow.AgregarProducto("Laptop", 650.00m, 10);
    }

    private PedidoGuardarDto Pedido(int cantidad, DateTime? fecha = null) => new()
    {
        ClienteId = _cliente.Id,
        ProductoId = _producto.Id,
        Cantidad = cantidad,
        FechaPedido = fecha
    };

    // Caso 5: Creación de un pedido.
    [Fact]
    public async Task CrearPedido_ConDatosValidos_GuardaPedidoYCalculaTotal()
    {
        var fecha = new DateTime(2026, 10, 10, 9, 30, 0);

        var creado = await _servicio.CrearAsync(Pedido(cantidad: 2, fecha: fecha));

        Assert.True(creado.Id > 0);
        Assert.Equal(fecha, creado.FechaPedido);
        Assert.Equal(2, creado.Cantidad);
        Assert.Equal(_cliente.Id, creado.ClienteId);
        Assert.Equal(_producto.Id, creado.ProductoId);
        Assert.Equal("Nelson Rivas", creado.Cliente);
        Assert.Equal(650.00m, creado.PrecioUnitario);
        Assert.Equal(1300.00m, creado.Total);
        Assert.Single(_uow.TablaPedidos);
    }

    [Fact]
    public async Task CrearPedido_SinFecha_UsaLaFechaActual()
    {
        var antes = DateTime.Now.AddSeconds(-1);

        var creado = await _servicio.CrearAsync(Pedido(cantidad: 1));

        Assert.True(creado.FechaPedido >= antes && creado.FechaPedido <= DateTime.Now.AddSeconds(1));
    }

    // Caso 6: Consulta de un pedido existente.
    [Fact]
    public async Task ObtenerPedido_Existente_DevuelveSusDatos()
    {
        var creado = await _servicio.CrearAsync(Pedido(cantidad: 3));

        var consultado = await _servicio.ObtenerPorIdAsync(creado.Id);

        Assert.Equal(creado.Id, consultado.Id);
        Assert.Equal(3, consultado.Cantidad);
        Assert.Equal("Laptop", consultado.Producto);
        Assert.Equal(1950.00m, consultado.Total);
    }

    // Caso 7: Intento de consultar un registro inexistente.
    [Fact]
    public async Task ObtenerPedido_Inexistente_LanzaRecursoNoEncontrado()
    {
        var error = await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _servicio.ObtenerPorIdAsync(12345));

        Assert.Equal("pedido", error.Recurso);
        Assert.Equal(12345, error.IdBuscado);
    }

    // Caso 8: Validación de reglas de negocio.
    [Fact]
    public async Task CrearPedido_CantidadMayorAlStock_LanzaReglaDeNegocioYNoDescuentaStock()
    {
        var error = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.CrearAsync(Pedido(cantidad: 11)));

        Assert.Contains("Stock insuficiente", error.Message);
        Assert.Equal(10, _producto.Stock);
        Assert.Empty(_uow.TablaPedidos);
    }

    [Fact]
    public async Task CrearPedido_DescuentaElStockDelProducto()
    {
        await _servicio.CrearAsync(Pedido(cantidad: 4));

        Assert.Equal(6, _producto.Stock);
    }

    [Fact]
    public async Task CrearPedido_ConClienteInexistente_LanzaReglaDeNegocio()
    {
        var datos = Pedido(cantidad: 1);
        datos.ClienteId = 999;

        var error = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.CrearAsync(datos));

        Assert.Contains("cliente", error.Message);
    }

    [Fact]
    public async Task CrearPedido_ConFechaFutura_LanzaReglaDeNegocio()
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            _servicio.CrearAsync(Pedido(cantidad: 1, fecha: DateTime.Now.AddDays(2))));
    }

    [Fact]
    public async Task CrearPedido_ConCantidadCero_LanzaValidacion()
    {
        var error = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(Pedido(cantidad: 0)));

        Assert.True(error.Errores.ContainsKey(nameof(PedidoGuardarDto.Cantidad)));
    }

    [Fact]
    public async Task EliminarPedido_DevuelveLasUnidadesAlStock()
    {
        var creado = await _servicio.CrearAsync(Pedido(cantidad: 5));
        Assert.Equal(5, _producto.Stock);

        await _servicio.EliminarAsync(creado.Id);

        Assert.Equal(10, _producto.Stock);
        Assert.Empty(_uow.TablaPedidos);
    }

    [Fact]
    public async Task ActualizarPedido_AjustaElStockSegunLaNuevaCantidad()
    {
        var creado = await _servicio.CrearAsync(Pedido(cantidad: 2));

        var actualizado = await _servicio.ActualizarAsync(creado.Id, Pedido(cantidad: 7));

        Assert.Equal(7, actualizado.Cantidad);
        Assert.Equal(3, _producto.Stock);
        Assert.Equal(4550.00m, actualizado.Total);
    }

    [Fact]
    public async Task ActualizarPedido_SinStockSuficiente_NoModificaNada()
    {
        var creado = await _servicio.CrearAsync(Pedido(cantidad: 2));

        await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.ActualizarAsync(creado.Id, Pedido(cantidad: 11)));

        Assert.Equal(8, _producto.Stock);
        Assert.Equal(2, _uow.TablaPedidos[0].Cantidad);
    }
}
