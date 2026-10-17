using Desafio03.Entidades.Interfaces;
using Desafio03.Entidades.Modelos;
using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Excepciones;
using Desafio03.Negocio.Validacion;

namespace Desafio03.Negocio.Servicios;

public interface IClienteServicio
{
    Task<List<ClienteDto>> ObtenerTodosAsync();
    Task<ClienteDto> ObtenerPorIdAsync(int id);
    Task<List<PedidoDto>> ObtenerPedidosAsync(int clienteId);
    Task<ClienteDto> CrearAsync(ClienteGuardarDto datos);
    Task<ClienteDto> ActualizarAsync(int id, ClienteGuardarDto datos);
    Task EliminarAsync(int id);
}

/// <summary>
/// Lógica de negocio de clientes.
/// Reglas: el email es único y no se elimina un cliente que tenga pedidos.
/// </summary>
public class ClienteServicio : IClienteServicio
{
    private readonly IUnidadDeTrabajo _uow;

    public ClienteServicio(IUnidadDeTrabajo unidadDeTrabajo)
    {
        _uow = unidadDeTrabajo;
    }

    public async Task<List<ClienteDto>> ObtenerTodosAsync()
    {
        var clientes = await _uow.Clientes.ObtenerTodosAsync();
        return clientes.Select(ClienteDto.DesdeEntidad).ToList();
    }

    public async Task<ClienteDto> ObtenerPorIdAsync(int id)
    {
        var cliente = await BuscarAsync(id);
        return ClienteDto.DesdeEntidad(cliente);
    }

    public async Task<List<PedidoDto>> ObtenerPedidosAsync(int clienteId)
    {
        if (!await _uow.Clientes.ExisteAsync(clienteId))
        {
            throw new RecursoNoEncontradoException("cliente", clienteId);
        }

        var pedidos = await _uow.Pedidos.ObtenerPorClienteAsync(clienteId);
        return pedidos.Select(PedidoDto.DesdeEntidad).ToList();
    }

    public async Task<ClienteDto> CrearAsync(ClienteGuardarDto datos)
    {
        Normalizar(datos);
        Validador.Validar(datos);

        if (await _uow.Clientes.ExisteEmailAsync(datos.Email))
        {
            throw new ReglaNegocioException($"Ya existe un cliente registrado con el email {datos.Email}.");
        }

        var cliente = new Cliente
        {
            Nombre = datos.Nombre,
            Apellido = datos.Apellido,
            Email = datos.Email
        };

        Validador.Validar(cliente);
        _uow.Clientes.Agregar(cliente);
        await _uow.GuardarCambiosAsync();

        return ClienteDto.DesdeEntidad(cliente);
    }

    public async Task<ClienteDto> ActualizarAsync(int id, ClienteGuardarDto datos)
    {
        Normalizar(datos);
        Validador.Validar(datos);

        var cliente = await BuscarAsync(id);

        if (await _uow.Clientes.ExisteEmailAsync(datos.Email, excluirId: id))
        {
            throw new ReglaNegocioException($"Ya existe otro cliente registrado con el email {datos.Email}.");
        }

        cliente.Nombre = datos.Nombre;
        cliente.Apellido = datos.Apellido;
        cliente.Email = datos.Email;

        Validador.Validar(cliente);
        _uow.Clientes.Actualizar(cliente);
        await _uow.GuardarCambiosAsync();

        return ClienteDto.DesdeEntidad(cliente);
    }

    public async Task EliminarAsync(int id)
    {
        var cliente = await BuscarAsync(id);

        if (await _uow.Clientes.TienePedidosAsync(id))
        {
            throw new ReglaNegocioException(
                "No se puede eliminar el cliente porque tiene pedidos registrados. Elimine primero sus pedidos.");
        }

        _uow.Clientes.Eliminar(cliente);
        await _uow.GuardarCambiosAsync();
    }

    private async Task<Cliente> BuscarAsync(int id)
    {
        return await _uow.Clientes.ObtenerPorIdAsync(id)
               ?? throw new RecursoNoEncontradoException("cliente", id);
    }

    /// <summary>Quita espacios sobrantes y guarda el email en minúsculas.</summary>
    private static void Normalizar(ClienteGuardarDto datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        datos.Nombre = datos.Nombre?.Trim() ?? string.Empty;
        datos.Apellido = datos.Apellido?.Trim() ?? string.Empty;
        datos.Email = datos.Email?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
