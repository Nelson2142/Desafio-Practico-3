using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Excepciones;
using Desafio03.Negocio.Servicios;
using Desafio03.Negocio.Validacion;
using Desafio03.Pruebas.Fakes;

namespace Desafio03.Pruebas;

public class ClienteServicioPruebas
{
    private readonly UnidadDeTrabajoFalsa _uow = new();
    private readonly ClienteServicio _servicio;

    public ClienteServicioPruebas()
    {
        _servicio = new ClienteServicio(_uow);
    }

    // Caso 1: Creación de un cliente.
    [Fact]
    public async Task CrearCliente_ConDatosValidos_GuardaElCliente()
    {
        var datos = new ClienteGuardarDto { Nombre = "Nelson", Apellido = "Rivas", Email = "nelson.rivas@correo.com" };

        var creado = await _servicio.CrearAsync(datos);

        Assert.True(creado.Id > 0);
        Assert.Equal("Nelson", creado.Nombre);
        Assert.Equal("Rivas", creado.Apellido);
        Assert.Equal("nelson.rivas@correo.com", creado.Email);
        Assert.Single(_uow.TablaClientes);
        Assert.Equal(1, _uow.GuardadosRealizados);
    }

    [Fact]
    public async Task CrearCliente_NormalizaEspaciosYEmailEnMinusculas()
    {
        var datos = new ClienteGuardarDto { Nombre = "  Steven ", Apellido = " Tobar  ", Email = "  Steven.TOBAR@Correo.com " };

        var creado = await _servicio.CrearAsync(datos);

        Assert.Equal("Steven", creado.Nombre);
        Assert.Equal("Tobar", creado.Apellido);
        Assert.Equal("steven.tobar@correo.com", creado.Email);
    }

    // Caso 2: Validación de datos incorrectos de un cliente.
    [Fact]
    public async Task CrearCliente_ConDatosIncorrectos_LanzaValidacionConErroresPorCampo()
    {
        var datos = new ClienteGuardarDto { Nombre = "", Apellido = "R2D2", Email = "correo-invalido" };

        var error = await Assert.ThrowsAsync<ValidacionException>(() => _servicio.CrearAsync(datos));

        Assert.True(error.Errores.ContainsKey(nameof(ClienteGuardarDto.Nombre)));
        Assert.True(error.Errores.ContainsKey(nameof(ClienteGuardarDto.Apellido)));
        Assert.True(error.Errores.ContainsKey(nameof(ClienteGuardarDto.Email)));
        Assert.Empty(_uow.TablaClientes);
        Assert.Equal(0, _uow.GuardadosRealizados);
    }

    [Theory]
    [InlineData("sin-arroba.com")]
    [InlineData("usuario@")]
    [InlineData("usuario@dominio")]
    [InlineData("")]
    public void ValidarCliente_EmailConFormatoIncorrecto_EsRechazadoPorDataAnnotations(string email)
    {
        var datos = new ClienteGuardarDto { Nombre = "Ana", Apellido = "López", Email = email };

        var errores = Validador.ObtenerErrores(datos);

        Assert.True(errores.ContainsKey(nameof(ClienteGuardarDto.Email)));
    }

    [Fact]
    public void ValidarCliente_NombreDemasiadoLargo_EsRechazado()
    {
        var datos = new ClienteGuardarDto { Nombre = new string('A', 51), Apellido = "López", Email = "ana@correo.com" };

        var errores = Validador.ObtenerErrores(datos);

        Assert.True(errores.ContainsKey(nameof(ClienteGuardarDto.Nombre)));
    }

    // Regla de negocio: el email debe ser único.
    [Fact]
    public async Task CrearCliente_ConEmailRepetido_LanzaReglaDeNegocio()
    {
        _uow.AgregarCliente("Ana", "López", "ana@correo.com");
        var datos = new ClienteGuardarDto { Nombre = "Ana María", Apellido = "Pérez", Email = "ANA@correo.com" };

        var error = await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.CrearAsync(datos));

        Assert.Contains("ana@correo.com", error.Message);
        Assert.Single(_uow.TablaClientes);
    }

    // Caso 7: Intento de consultar un registro inexistente.
    [Fact]
    public async Task ObtenerCliente_Inexistente_LanzaRecursoNoEncontrado()
    {
        var error = await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _servicio.ObtenerPorIdAsync(999));

        Assert.Equal(999, error.IdBuscado);
    }

    [Fact]
    public async Task EliminarCliente_ConPedidos_LanzaReglaDeNegocio()
    {
        var cliente = _uow.AgregarCliente("Ana", "López", "ana@correo.com");
        var producto = _uow.AgregarProducto("Mouse", 10m, 5);
        await new PedidoServicio(_uow).CrearAsync(new PedidoGuardarDto
        {
            ClienteId = cliente.Id,
            ProductoId = producto.Id,
            Cantidad = 1
        });

        await Assert.ThrowsAsync<ReglaNegocioException>(() => _servicio.EliminarAsync(cliente.Id));

        Assert.Single(_uow.TablaClientes);
    }

    [Fact]
    public async Task ActualizarCliente_ConDatosValidos_ModificaLosCampos()
    {
        var cliente = _uow.AgregarCliente("Ana", "López", "ana@correo.com");

        var actualizado = await _servicio.ActualizarAsync(cliente.Id,
            new ClienteGuardarDto { Nombre = "Ana Sofía", Apellido = "López", Email = "anasofia@correo.com" });

        Assert.Equal("Ana Sofía", actualizado.Nombre);
        Assert.Equal("anasofia@correo.com", _uow.TablaClientes[0].Email);
    }
}
