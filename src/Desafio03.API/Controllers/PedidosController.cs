using Desafio03.Negocio.DTOs;
using Desafio03.Negocio.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Desafio03.API.Controllers;

/// <summary>Gestión de pedidos (descuenta y devuelve stock automáticamente).</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoServicio _servicio;

    public PedidosController(IPedidoServicio servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Lista todos los pedidos (más recientes primero).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PedidoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PedidoDto>>> ObtenerTodos()
    {
        return Ok(await _servicio.ObtenerTodosAsync());
    }

    /// <summary>Obtiene un pedido por su Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoDto>> ObtenerPorId(int id)
    {
        return Ok(await _servicio.ObtenerPorIdAsync(id));
    }

    /// <summary>Registra un pedido y descuenta el stock del producto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDto>> Crear([FromBody] PedidoGuardarDto datos)
    {
        var creado = await _servicio.CrearAsync(datos);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    /// <summary>Modifica un pedido y ajusta el stock.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDto>> Actualizar(int id, [FromBody] PedidoGuardarDto datos)
    {
        return Ok(await _servicio.ActualizarAsync(id, datos));
    }

    /// <summary>Cancela un pedido y devuelve las unidades al stock.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}
