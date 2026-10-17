using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Desafio03.API.Controllers;

/// <summary>CRUD de clientes.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteServicio _servicio;

    public ClientesController(IClienteServicio servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Lista todos los clientes.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClienteDto>>> ObtenerTodos()
    {
        return Ok(await _servicio.ObtenerTodosAsync());
    }

    /// <summary>Obtiene un cliente por su Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(int id)
    {
        return Ok(await _servicio.ObtenerPorIdAsync(id));
    }

    /// <summary>Lista los pedidos de un cliente.</summary>
    [HttpGet("{id:int}/pedidos")]
    [ProducesResponseType(typeof(List<PedidoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PedidoDto>>> ObtenerPedidos(int id)
    {
        return Ok(await _servicio.ObtenerPedidosAsync(id));
    }

    /// <summary>Crea un cliente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Crear([FromBody] ClienteGuardarDto datos)
    {
        var creado = await _servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    /// <summary>Actualiza un cliente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, [FromBody] ClienteGuardarDto datos)
    {
        return Ok(await _servicio.ActualizarAsync(id, datos));
    }

    /// <summary>Elimina un cliente (solo si no tiene pedidos).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}
