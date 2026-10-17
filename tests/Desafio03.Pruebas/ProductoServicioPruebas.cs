using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Excepciones;
using Desafio03.Negocio.Servicios;
using Desafio03.Negocio.Validacion;
using Desafio03.Pruebas.Fakes;

namespace Desafio03.Pruebas;

public class ProductoServicioPruebas
{
    private readonly UnidadDeTrabajoFalsa _uow = new();
    private readonly ProductoServicio _servicio;

    public ProductoServicioPruebas()
    {
        _servicio = new ProductoServicio(_uow);
    }

    private static ProductoGuardarDto ProductoValido(decimal precio = 25.50m, int stock = 10) => new()
    {
        Nombre = "Teclado mecánico",
        Descripcion = "Teclado retroiluminado en español",
        Precio = precio,
        Stock = stock
    };

    // Caso 3: Creación de un producto.
    [Fact]
    public async Task CrearProducto_ConDatosValidos_GuardaElProducto()
    {
        var creado = await _servicio.CrearAsync(ProductoValido());

        Assert.True(creado.Id > 0);
        Assert.Equal("Teclado mecánico", creado.Nombre);
        Assert.Equal(25.50m, creado.Precio);
        Assert.Equal(10, creado.Stock);
        Assert.Single(_uow.TablaProductos);
    }

    // Caso 4: Validación del precio y stock.
    [Theory]
    [InlineData("0")]
    [InlineData("-5.25")]
    [InlineData("1000000")]
    public async Task CrearProducto_ConPrecioFueraDeRango_LanzaValidacion(string textoPrecio)
    {
        // Los decimal no pueden ir directamente en un atributo, por eso se envían como texto.
        var precio = decimal.Parse(textoPrecio, System.Globalization.CultureInfo.InvariantCulture);

        var error = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(ProductoValido(precio: precio)));

        Assert.True(error.Errores.ContainsKey(nameof(ProductoGuardarDto.Precio)));
        Assert.Empty(_uow.TablaProductos);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100001)]
    public async Task CrearProducto_ConStockFueraDeRango_LanzaValidacion(int stock)
    {
        var error = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(ProductoValido(stock: stock)));

        Assert.True(error.Errores.ContainsKey(nameof(ProductoGuardarDto.Stock)));
        Assert.Empty(_uow.TablaProductos);
    }

    [Fact]
    public void ValidarProducto_PrecioYStockEnElLimite_SonValidos()
    {
        Assert.Empty(Validador.ObtenerErrores(ProductoValido(precio: 0.01m, stock: 0)));
        Assert.Empty(Validador.ObtenerErrores(ProductoValido(precio: 999999.99m, stock: 100000)));
    }

    [Fact]
    public async Task CrearProducto_RedondeaElPrecioADosDecimales()
    {
        var creado = await _servicio.CrearAsync(ProductoValido(precio: 10.456m));

        Assert.Equal(10.46m, creado.Precio);
    }

    [Fact]
    public async Task ObtenerProducto_Inexistente_LanzaRecursoNoEncontrado()
    {
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _servicio.ObtenerPorIdAsync(42));
    }

    [Fact]
    public async Task ActualizarProducto_CambiaPrecioYStock()
    {
        var producto = _uow.AgregarProducto("Monitor", 100m, 3);

        var actualizado = await _servicio.ActualizarAsync(producto.Id, ProductoValido(precio: 120m, stock: 8));

        Assert.Equal(120m, actualizado.Precio);
        Assert.Equal(8, _uow.TablaProductos[0].Stock);
    }
}
